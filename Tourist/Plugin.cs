using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVWeather.Lumina;

namespace Tourist {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public static string Name => "Tourist";

        [PluginService]
        internal static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        internal IDalamudPluginInterface Interface { get; init; }

        [PluginService]
        internal IClientState ClientState { get; init; } = null!;

        [PluginService]
        internal ICommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal IDataManager DataManager { get; init; } = null!;

        [PluginService]
        internal IFramework Framework { get; init; } = null!;

        [PluginService]
        internal IGameGui GameGui { get; init; } = null!;

        [PluginService]
        internal ISigScanner SigScanner { get; init; } = null!;

        [PluginService]
        internal IGameInteropProvider GameInteropProvider { get; init; } = null!;

        internal Configuration Config { get; }
        internal PluginUi Ui { get; }
        internal FFXIVWeatherLuminaService Weather { get; }
        internal GameFunctions Functions { get; }
        private Commands Commands { get; }
        internal Markers Markers { get; }

        public Plugin(IDalamudPluginInterface pluginInterface) {
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
