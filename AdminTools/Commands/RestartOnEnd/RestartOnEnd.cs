using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;

namespace AdminTools.Commands.PlayerBroadcast
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class RestartOnEnd : ParentCommand
    {
        public RestartOnEnd() => LoadGeneratedCommands();

        public override string Command { get; } = "restartonend";

        public override string[] Aliases { get; } = new string[] { "roe" };

        public override string Description { get; } = "Restarts the server on round end to not effect gameplay.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "restartonend", PlayerPermissions.ServerConsoleCommands, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            Plugin.RestartOnEnd = !Plugin.RestartOnEnd;
            response = $"The server will{(Plugin.RestartOnEnd ? " " : " no longer ")}automatically restart the next time the round ends!";

            return true;
        }
    }
}
