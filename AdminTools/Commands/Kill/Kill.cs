using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Kill
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Kill : ParentCommand
    {
        public Kill() => LoadGeneratedCommands();

        public override string Command { get; } = "atkill";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Kills everyone or a user instantly";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.kill"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: kill ((player id / name) or (all / *))";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    foreach (Player ply in Player.List)
                    {
                        if (ply.Role == RoleType.Spectator || ply.Role == RoleType.None)
                            continue;

                        ply.Kill("Killed by admin.");
                    }

                    response = "Everyone has been game ended (killed) now";
                    return true;
                default:
                    Player pl = Player.Get(arguments.At(0));
                    if (pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }
                    else if (pl.Role == RoleType.Spectator || pl.Role == RoleType.None)
                    {
                        response = $"Player {pl.Nickname} is not a valid class to kill";
                        return false;
                    }

                    pl.Kill("Killed by admin.");
                    response = $"Player {pl.Nickname} has been game ended (killed) now";
                    return true;
            }
        }
    }
}
