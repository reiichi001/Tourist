using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Tourist {
    public class PluginUi : IDisposable {
        private Plugin Plugin { get; }

        private bool _show;

        internal bool Show {
            get => this._show;
            set => this._show = value;
        }

        public PluginUi(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.UiBuilder.OnBuildUi += this.Draw;
            this.Plugin.Interface.UiBuilder.OnOpenConfigUi += this.OpenConfig;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OnOpenConfigUi -= this.OpenConfig;
            this.Plugin.Interface.UiBuilder.OnBuildUi -= this.Draw;
        }

        private void OpenConfig(object sender, EventArgs eventArgs) {
            this.Show = true;
        }

        private void Draw() {
            ImGui.SetNextWindowSize(new Vector2(350f, 450f), ImGuiCond.FirstUseEver);

            if (!this.Show || !ImGui.Begin(this.Plugin.Name, ref this._show, ImGuiWindowFlags.MenuBar)) {
                return;
            }

            if (ImGui.BeginMenuBar()) {
                if (ImGui.BeginMenu("Options")) {
                    if (ImGui.BeginMenu("Times")) {
                        var showTimeUntil = this.Plugin.Config.ShowTimeUntilAvailable;
                        if (ImGui.MenuItem("Show time until available", null, ref showTimeUntil)) {
                            this.Plugin.Config.ShowTimeUntilAvailable = showTimeUntil;
                            this.Plugin.Config.Save();
                        }

                        var showTimeLeft = this.Plugin.Config.ShowTimeLeft;
                        if (ImGui.MenuItem("Show time left", null, ref showTimeLeft)) {
                            this.Plugin.Config.ShowTimeLeft = showTimeLeft;
                            this.Plugin.Config.Save();
                        }

                        ImGui.EndMenu();
                    }

                    var showFinished = this.Plugin.Config.ShowFinished;
                    if (ImGui.MenuItem("Show finished", null, ref showFinished)) {
                        this.Plugin.Config.ShowFinished = showFinished;
                        this.Plugin.Config.Save();
                    }

                    var showUnavailable = this.Plugin.Config.ShowUnavailable;
                    if (ImGui.MenuItem("Show unavailable", null, ref showUnavailable)) {
                        this.Plugin.Config.ShowUnavailable = showUnavailable;
                        this.Plugin.Config.Save();
                    }

                    var showArrVistas = this.Plugin.Config.ShowArrVistas;
                    if (ImGui.MenuItem("Add markers for ARR vistas", null, ref showArrVistas)) {
                        this.Plugin.Config.ShowArrVistas = showArrVistas;
                        this.Plugin.Config.Save();

                        if (showArrVistas) {
                            var territory = this.Plugin.Interface.ClientState.TerritoryType;
                            this.Plugin.Markers.SpawnVfxForCurrentZone(territory);
                        } else {
                            this.Plugin.Markers.RemoveAllVfx();
                        }
                    }

                    var onlyCurrent = this.Plugin.Config.OnlyShowCurrentZone;
                    if (ImGui.MenuItem("Only show vistas for current zone", null, ref onlyCurrent)) {
                        this.Plugin.Config.OnlyShowCurrentZone = onlyCurrent;
                        this.Plugin.Config.Save();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help")) {
                    if (ImGui.BeginMenu("Can't unlock vistas 21 to 80")) {
                        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 10);
                        ImGui.TextUnformatted("Vistas 21 to 80 require the completion of the first 20. Talk to Millith Ironheart in Old Gridania to unlock the rest.");
                        ImGui.PopTextWrapPos();

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (ImGui.BeginChild("tourist-adventures", new Vector2(0, 0))) {
                var adventures = this.Plugin.Interface.Data.GetExcelSheet<Adventure>();

                var row = 0;
                foreach (var adventure in adventures) {
                    var idx = row;
                    row += 1;

                    if (this.Plugin.Config.OnlyShowCurrentZone && adventure.Level.Value.Territory.Row != this.Plugin.Interface.ClientState.TerritoryType) {
                        continue;
                    }

                    var has = this.Plugin.Functions.HasVistaUnlocked((short) idx);

                    if (!this.Plugin.Config.ShowFinished && has) {
                        continue;
                    }

                    var available = adventure.Available(this.Plugin.Weather);

                    if (!this.Plugin.Config.ShowUnavailable && !available) {
                        continue;
                    }

                    var availability = adventure.NextAvailable(this.Plugin.Weather);

                    DateTimeOffset? countdown = null;
                    if (has) {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.8f, 0.8f, 1f));
                    } else if (available) {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 1f, 0f, 1f));
                        if (this.Plugin.Config.ShowTimeLeft) {
                            countdown = availability?.end;
                        }
                    } else if (availability != null && this.Plugin.Config.ShowTimeUntilAvailable) {
                        countdown = availability.Value.start;
                    }

                    var next = countdown == null
                        ? string.Empty
                        : $" ({(countdown.Value - DateTimeOffset.UtcNow).ToHumanReadable()})";

                    var name = this.Plugin.Interface.SeStringManager.Parse(adventure.Name.RawData.ToArray());
                    var header = ImGui.CollapsingHeader($"#{row} - {name.TextValue}{next}###adventure-{adventure.RowId}");

                    if (has || available) {
                        ImGui.PopStyleColor();
                    }

                    if (!header) {
                        continue;
                    }

                    ImGui.Columns(2);
                    ImGui.SetColumnWidth(0, ImGui.CalcTextSize("Eorzea time").X + ImGui.GetStyle().ItemSpacing.X * 2);

                    ImGui.TextUnformatted("Command");
                    ImGui.NextColumn();

                    ImGui.TextUnformatted(adventure.Emote.Value.TextCommand.Value.Command);
                    ImGui.NextColumn();

                    ImGui.TextUnformatted("Eorzea time");
                    ImGui.NextColumn();

                    if (adventure.MinTime != 0 && adventure.MaxTime != 0) {
                        ImGui.TextUnformatted($"{adventure.MinTime / 100:00}:00 to {adventure.MaxTime / 100 + 1:00}:00");
                    } else {
                        ImGui.TextUnformatted("Any");
                    }

                    ImGui.NextColumn();

                    ImGui.TextUnformatted("Weather");
                    ImGui.NextColumn();

                    if (Weathers.All.TryGetValue(adventure.RowId, out var weathers)) {
                        var weatherString = string.Join(", ", weathers
                            .OrderBy(id => id)
                            .Select(id => this.Plugin.Interface.Data.GetExcelSheet<Weather>().GetRow(id))
                            .Select(weather => weather.Name));
                        ImGui.TextUnformatted(weatherString);
                    } else {
                        ImGui.TextUnformatted("Any");
                    }

                    ImGui.Columns();

                    if (ImGui.Button($"Open map##{adventure.RowId}")) {
                        this.Plugin.Interface.OpenMapLocation(adventure);
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
    }
}
