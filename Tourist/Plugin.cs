using System;
using System.Reflection;
using Dalamud.Plugin;
using FFXIVWeather.Lumina;

namespace Tourist {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Tourist";

        internal DalamudPluginInterface Interface { get; private set; } = null!;
        internal Configuration Config { get; private set; } = null!;
        internal PluginUi Ui { get; private set; } = null!;
        internal FFXIVWeatherLuminaService Weather { get; private set; } = null!;
        internal GameFunctions Functions { get; private set; } = null!;
        private Commands Commands { get; set; } = null!;
        internal Markers Markers { get; private set; } = null!;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.Interface = pluginInterface;

            this.Config = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.Initialise(this);

            var gameDataField = this.Interface.Data.GetType().GetField("gameData", BindingFlags.Instance | BindingFlags.NonPublic);
            if (gameDataField == null) {
                throw new Exception("Missing gameData field");
            }

            var lumina = (Lumina.Lumina) gameDataField.GetValue(this.Interface.Data);
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
