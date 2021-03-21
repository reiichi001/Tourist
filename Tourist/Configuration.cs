using System;
using Dalamud.Configuration;

namespace Tourist {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        private Plugin Plugin { get; set; } = null!;

        public int Version { get; set; } = 1;

        public bool ShowFinished { get; set; } = true;
        public bool ShowUnavailable { get; set; } = true;
        public bool ShowArrVistas { get; set; } = true;
        public bool OnlyShowCurrentZone { get; set; }
        public bool ShowTimeUntilAvailable { get; set; } = true;
        public bool ShowTimeLeft { get; set; } = true;

        internal void Initialise(Plugin plugin) {
            this.Plugin = plugin;
        }

        internal void Save() {
            this.Plugin.Interface.SavePluginConfig(this);
        }
    }
}
