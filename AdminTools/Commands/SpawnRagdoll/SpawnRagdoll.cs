using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.SpawnRagdoll
{
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
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
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

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    if (!Enum.TryParse(arguments.At(0), true, out RoleType role))
                    {
                        response = $"Invalid value for role type: {arguments.At(0)}";
                        return false;
                    }

                    if (!uint.TryParse(arguments.At(1), out uint amount))
                    {
                        response = $"Invalid value for ragdoll amount: {arguments.At(1)}";
                        return false;
                    }

                    foreach (Player ply in Player.List)
                    {
                        if (ply.Role == RoleType.Spectator || ply.Role == RoleType.None)
                            continue;

                        Timing.RunCoroutine(EventHandlers.SpawnBodies(ply, role, (int)amount));
                    }

                    response = $"{amount} {role.ToString()} ragdolls have spawned on everyone";
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
                        response = $"This player is not a valid class to spawn a ragdoll on";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out RoleType r))
                    {
                        response = $"Invalid value for role type: {arguments.At(1)}";
                        return false;
                    }

                    if (!uint.TryParse(arguments.At(2), out uint count))
                    {
                        response = $"Invalid value for ragdoll amount: {arguments.At(2)}";
                        return false;
                    }

                    Timing.RunCoroutine(EventHandlers.SpawnBodies(pl, r, (int)count));
                    response = $"{count} {r.ToString()} ragdolls have spawned on Player {pl.Nickname}";
                    return true;
            }
        }
    }
}
