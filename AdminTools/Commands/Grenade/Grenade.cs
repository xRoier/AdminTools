﻿using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using PlayerRoles;

namespace AdminTools.Commands.Grenade
{
    using Exiled.API.Enums;
    using Exiled.API.Features.Items;
    using Exiled.API.Extensions;
    using InventorySystem.Items.ThrowableProjectiles;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Grenade : ParentCommand
    {
        public Grenade() => LoadGeneratedCommands();

        public override string Command { get; } = "grenade";

        public override string[] Aliases { get; } = new string[] { "gn" };

        public override string Description { get; } = "Spawns a frag/flash/scp018 grenade on a user or users";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.grenade"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2 || arguments.Count > 3)
            {
                response = "Usage: grenade ((player id / name) or (all / *)) (GrenadeType) (grenade time)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out GrenadeType type))
            {
                response = $"Invalid value for grenade type: {arguments.At(1)}";
                return false;
            }

            if (!float.TryParse(arguments.At(2), out float fuseTime))
            {
                response = $"Invalid fuse time for grenade: {arguments.At(2)}";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    if (type == GrenadeType.Scp018)
                        Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);
                    
                    foreach (Player player in Player.List)
                    {
                        if (player.Role.Type != RoleTypeId.Spectator) 
                            SpawnGrenade(player, type, fuseTime);
                    }

                    break;
                default:
                    Player ply = Player.Get(arguments.At(0));
                    if (ply is null)
                    {
                        response = $"Player {arguments.At(0)} not found.";
                        return false;
                    }

                    SpawnGrenade(ply, type, fuseTime);
                    break;
            }

            response = $"Grenade has been sent to {arguments.At(0)}";
            return true;
        }

        private void SpawnGrenade(Player player, GrenadeType type, float fuseTime)
        {
            switch (type)
            {
                case GrenadeType.Flashbang:
                    FlashGrenade flash = (FlashGrenade) Item.Create(ItemType.GrenadeFlash);
                    flash.FuseTime = fuseTime;
                    flash.SpawnActive(player.Position);

                    break;
                default:
                    ExplosiveGrenade grenade = (ExplosiveGrenade) Item.Create(type.GetItemType());
                    grenade.FuseTime = fuseTime;
                    grenade.SpawnActive(player.Position);
                    break;
            }
        }
    }
}
