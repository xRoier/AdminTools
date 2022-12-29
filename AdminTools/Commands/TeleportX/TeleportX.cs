﻿using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using PlayerRoles;

namespace AdminTools.Commands.TeleportX
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class TeleportX : ParentCommand
    {
        public TeleportX() => LoadGeneratedCommands();

        public override string Command { get; } = "teleportx";

        public override string[] Aliases { get; } = new string[] { "tpx" };

        public override string Description { get; } = "Teleports all users or a user to another user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.tp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: teleportx (People teleported: (player id / name) or (all / *)) (Teleported to: (player id / name) or (all / *))";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    Player ply = Player.Get(arguments.At(1));
                    if (ply == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }


                    foreach (Player plyr in Player.List)
                    {
                        if (plyr.Role.Type== RoleTypeId.Spectator || ply.Role.Type== RoleTypeId.None)
                            continue;

                        plyr.Position = ply.Position;
                    }

                    response = $"Everyone has been teleported to Player {ply.Nickname}";
                    return true;
                default:
                    Player pl = Player.Get(arguments.At(0));
                    if (pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    Player plr = Player.Get(arguments.At(1));
                    if (plr == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    pl.Position = plr.Position;
                    response = $"Player {pl.Nickname} has been teleported to Player {plr.Nickname}";
                    return true;
            }
        }
    }
}
