using System;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using FFXIVWeather.Lumina;

namespace Tourist {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Tourist";

        internal DalamudPluginInterface Interface { get; }
        internal ClientState ClientState { get; }
        internal CommandManager CommandManager { get; }
        internal DataManager DataManager { get; }
        internal Framework Framework { get; }
        internal GameGui GameGui { get; }
        internal SeStringManager SeStringManager { get; }
        internal SigScanner SigScanner { get; }

        internal Configuration Config { get; }
        internal PluginUi Ui { get; }
        internal FFXIVWeatherLuminaService Weather { get; }
        internal GameFunctions Functions { get; }
        private Commands Commands { get; }
        internal Markers Markers { get; }

        public Plugin(
            DalamudPluginInterface pluginInterface,
            ClientState clientState,
            CommandManager commandManager,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            SeStringManager seStringManager,
            SigScanner sigScanner
        ) {
            this.Interface = pluginInterface;
            this.ClientState = clientState;
            this.CommandManager = commandManager;
            this.DataManager = dataManager;
            this.Framework = framework;
            this.GameGui = gameGui;
            this.SeStringManager = seStringManager;
            this.SigScanner = sigScanner;

            this.Config = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.Initialise(this);

            var gameDataField = this.DataManager.GetType().GetField("gameData", BindingFlags.Instance | BindingFlags.NonPublic);
            if (gameDataField == null) {
                throw new Exception("Missing gameData field");
            }

            var lumina = (Lumina.GameData) gameDataField.GetValue(this.DataManager)!;
            this.Weather = new FFXIVWeatherLuminaService(lumina);

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
