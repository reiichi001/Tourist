using System;
using Dalamud.Game.Command;

namespace Tourist {
    public class Commands : IDisposable {
        private Plugin Plugin { get; }

        public Commands(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.CommandManager.AddHandler("/tourist", new CommandInfo(this.OnCommand) {
                HelpMessage = "Opens the Tourist interface",
            });
        }

        public void Dispose() {
            this.Plugin.CommandManager.RemoveHandler("/tourist");
        }

        private void OnCommand(string command, string arguments) {
            this.Plugin.Ui.Show = !this.Plugin.Ui.Show;
        }
    }
}
