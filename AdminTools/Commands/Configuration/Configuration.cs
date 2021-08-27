using CommandSystem;
using System;

namespace AdminTools.Commands.Configuration
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Configuration : ParentCommand
    {
        public Configuration() => LoadGeneratedCommands();

        public override string Command { get; } = "cfig";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Manages reloading permissions and configs";

        public override void LoadGeneratedCommands() 
        {
            RegisterCommand(new Reload());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid subcommmand. Available ones: reload";
            return false;
        }
    }
}
