using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.SpawnRagdoll
{
    using System.Collections.Generic;
    using Mirror;
    using PlayerStatsSystem;
    using UnityEngine;
    using Ragdoll = Exiled.API.Features.Ragdoll;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SpawnRagdoll : ParentCommand
    {
        public SpawnRagdoll() => LoadGeneratedCommands();

        public override string Command { get; } = "spawnragdoll";

        public override string[] Aliases { get; } = new string[] { "ragdoll", "rd", "rag", "doll" };

        public override string Description { get; } = "Spawns a specified number of ragdolls on a user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.dolls"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 3)
            {
                response = "Usage: spawnragdoll ((player id / name) or (all / *)) (RoleType) (amount)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out RoleType type))
            {
                response = $"Invalid RoleType for ragdoll: {arguments.At(1)}";
                return false;
            }

            if (!int.TryParse(arguments.At(2), out int amount))
            {
                response = $"Invalid amount of ragdolls to spawn: {arguments.At(2)}";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    foreach (Player player in Player.List)
                    {
                        if (player.Role != RoleType.Spectator) 
                            Timing.RunCoroutine(SpawnDolls(player, type, amount));
                    }

                    break;
                default:
                    Player ply = Player.Get(arguments.At(0));
                    if (ply is null)
                    {
                        response = $"Player {arguments.At(0)} not found.";
                        return false;
                    }

                    Timing.RunCoroutine(SpawnDolls(ply, type, amount));

                    break;
            }

            response = $"{amount} {type} ragdoll(s) have been spawned on {arguments.At(0)}.";
            return true;
        }

        private IEnumerator<float> SpawnDolls(Player player, RoleType type, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                new Ragdoll(new RagdollInfo(Server.Host.ReferenceHub, new UniversalDamageHandler(200, DeathTranslations.Crushed), type, player.Position + (Vector3.up * 3f), default, "SCP-343", NetworkTime.time), true);
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
