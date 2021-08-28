using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVWeather.Lumina;

namespace Tourist {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Tourist";

        internal DalamudPluginInterface Interface { get; }

        [PluginService]
        internal ClientState ClientState { get; init; } = null!;

        [PluginService]
        internal CommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal DataManager DataManager { get; init; } = null!;

        [PluginService]
        internal Framework Framework { get; init; } = null!;

        [PluginService]
        internal GameGui GameGui { get; init; } = null!;

        [PluginService]
        internal SigScanner SigScanner { get; init; } = null!;

        internal Configuration Config { get; }
        internal PluginUi Ui { get; }
        internal FFXIVWeatherLuminaService Weather { get; }
        internal GameFunctions Functions { get; }
        private Commands Commands { get; }
        internal Markers Markers { get; }

        public Plugin(DalamudPluginInterface pluginInterface) {
            this.Interface = pluginInterface;

            this.Config = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.Initialise(this);

            this.Weather = new FFXIVWeatherLuminaService(this.DataManager.GameData);

            this.Functions = new GameFunctions(this);

            this.Markers = new Markers(this);

            this.Ui = new PluginUi(this);

            this.Commands = new Commands(this);
        }

        public void Dispose() {
            this.Commands.Dispose();
            this.Ui.Dispose();
            this.Markers.Dispose();
            this.Functions.Dispose();
        }
    }
}
