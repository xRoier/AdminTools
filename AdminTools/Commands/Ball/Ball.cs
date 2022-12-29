﻿using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using PlayerRoles;

namespace AdminTools.Commands.Ball
{
    using System.Collections.Generic;
    using Exiled.API.Features.Items;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Ball : ParentCommand
    {
        public Ball() => LoadGeneratedCommands();

        public override string Command { get; } = "ball";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Spawns a bouncy ball (SCP-018) on a user or all users";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.ball"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: ball ((player id/ name) or (all / *))";
                return false;
            }

            List<Player> players = new();
            switch (arguments.At(0)) 
            {
                case "*":
                case "all":
                    foreach (Player pl in Player.List)
                    {
                        if (pl.Role.Type== RoleTypeId.Spectator || pl.Role.Type== RoleTypeId.None)
                            continue;

                        players.Add(pl);
                    }

                    break;
                default:
                    Player ply = Player.Get(arguments.At(0));
                    if (ply == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (ply.Role.Type== RoleTypeId.Spectator || ply.Role.Type== RoleTypeId.None)
                    {
                        response = $"You cannot spawn a ball on that player right now";
                        return false;
                    }

                    players.Add(ply);
                    break;
            }

            response = players.Count == 1
                ? $"{players[0].Nickname} has received a bouncing ball!"
                : $"The balls are bouncing for {players.Count} players!";
            if (players.Count > 1)
                Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);

            foreach (Player p in players)
                ((ExplosiveGrenade)Item.Create(ItemType.SCP018)).SpawnActive(p.Position, p);
            return true;
        }
    }
}
