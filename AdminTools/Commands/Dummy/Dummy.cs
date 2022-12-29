/*
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using PlayerRoles;
using UnityEngine;

namespace AdminTools.Commands.Dummy
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Dummy : ParentCommand
    {
        public Dummy() => LoadGeneratedCommands();

        public override string Command { get; } = "dummy";

        public override string[] Aliases { get; } = new string[] { "dum" };

        public override string Description { get; } = "Spawns a dummy character on all users on a user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.dummy"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if(!(sender is PlayerCommandSender plysend))
            {
                response = "You must be in-game to run this command!";
                return false;
            }

            Player player = Player.Get(plysend.ReferenceHub);
            if (arguments.Count < 1)
            {
                response = "Usage:\ndummy ((player id / name) or (all / *)) (RoleTypeId) (x value) (y value) (z value)" +
                    "\ndummy clear (player id / name) (minimum index) (maximum index)" +
                    "\ndummy clearall" +
                    "\ndummy count (player id / name) ";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 4)
                    {
                        response = "Usage: dummy clear (player id / name) (minimum index) (maximum index)\nNote: Minimum < Maximum, you can remove from a range of dummies a user spawns";
                        return false;
                    }

                    Player ply = Player.Get(arguments.At(1));
                    if (ply == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (!int.TryParse(arguments.At(2), out int min) && min < 0)
                    {
                        response = $"Invalid value for minimum index: {arguments.At(2)}";
                        return false;
                    }

                    if (!int.TryParse(arguments.At(3), out int max) && max < 0)
                    {
                        response = $"Invalid value for maximum index: {arguments.At(3)}";
                        return false;
                    }

                    if (max < min)
                    {
                        response = $"{max} is not greater than {min}";
                        return false;
                    }

                    if (!Plugin.DumHubs.TryGetValue(ply, out List<GameObject> objs))
                    {
                        response = $"{ply.Nickname} has not spawned in any dummies in";
                        return false;
                    }

                    if (min > objs.Count)
                    {
                        response = $"{min} (minimum) is higher than the number of dummies {ply.Nickname} spawned! (Which is {objs.Count})";
                        return false;
                    }

                    if (max > objs.Count)
                    {
                        response = $"{max} (maximum) is higher than the number of dummies {ply.Nickname} spawned! (Which is {objs.Count})";
                        return false;
                    }

                    min = min == 0 ? 0 : min - 1;
                    max = max == 0 ? 0 : max - 1;

                    for (int i = min; i <= max; i++)
                    {
                        UnityEngine.Object.Destroy(objs.ElementAt(i));
                        objs[i] = null;
                    }
                    objs.RemoveAll(r => r == null);

                    response = $"All dummies from {min + 1} to {max + 1} have been cleared from Player {ply.Nickname}";
                    return true;
                case "clearall":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: dummy clearall";
                        return false;
                    }

                    foreach (KeyValuePair<Player, List<GameObject>> dummy in Plugin.DumHubs)
                    {
                        foreach (GameObject dum in dummy.Value)
                            UnityEngine.Object.Destroy(dum);
                        dummy.Value.Clear();
                    }

                    Plugin.DumHubs.Clear();
                    response = $"All spawned dummies have now been removed";
                    return true;
                case "count":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: dummy count (player id / name)";
                        return false;
                    }

                    Player plyr = Player.Get(arguments.At(1));
                    if (plyr == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (!Plugin.DumHubs.TryGetValue(plyr, out List<GameObject> obj) || obj.Count == 0)
                    {
                        response = $"{plyr.Nickname} has not spawned in any dummies in";
                        return false;
                    }

                    response = $"{plyr.Nickname} has spawned in {(obj.Count != 1 ? $"{obj.Count} dummies" : $"{obj.Count} dummy")}";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 5)
                    {
                        response = "Usage: dummy (all / *) (RoleTypeId) (x value) (y value) (z value)";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out RoleTypeId role))
                    {
                        response = $"Invalid value for role type: {arguments.At(1)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(2), out float xval))
                    {
                        response = $"Invalid x value for dummy size: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float yval))
                    {
                        response = $"Invalid y value for dummy size: {arguments.At(3)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(4), out float zval))
                    {
                        response = $"Invalid z value for dummy size: {arguments.At(4)}";
                        return false;
                    }
                    int index = 0;
                    foreach (Player p in Player.List)
                    {
                        if (p.Role.Type== RoleTypeId.Spectator || p.Role.Type== RoleTypeId.None)
                            continue;

                        EventHandlers.SpawnDummyModel(player, p.Position, p.GameObject.transform.localRotation, role, xval, yval, zval, out int dIndex);
                        index = dIndex;
                    }

                    response = $"A {role.ToString()} dummy has spawned on everyone, you now spawned in a total of {(index != 1 ? $"{index} dummies" : $"{index} dummies")}";
                    return true;
                default:
                    if (arguments.Count != 5)
                    {
                        response = "Usage: dummy (player id / name) (RoleTypeId) (x value) (y value) (z value)";
                        return false;
                    }

                    Player pl = Player.Get(arguments.At(0));
                    if (pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }
                    else if (pl.Role.Type== RoleTypeId.Spectator || pl.Role.Type== RoleTypeId.None)
                    {
                        response = $"This player is not a valid class to spawn a dummy on";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out RoleTypeId r2))
                    {
                        response = $"Invalid value for role type: {arguments.At(1)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(2), out float x))
                    {
                        response = $"Invalid x value for dummy size: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float y))
                    {
                        response = $"Invalid y value for dummy size: {arguments.At(3)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(4), out float z))
                    {
                        response = $"Invalid z value for dummy size: {arguments.At(4)}";
                        return false;
                    }

                    EventHandlers.SpawnDummyModel(player, pl.Position, pl.GameObject.transform.localRotation, r2, x, y, z, out int dummyIndex);
                    response = $"A {r2.ToString()} dummy has spawned on Player {pl.Nickname}, you now spawned in a total of {(dummyIndex != 1 ? $"{dummyIndex} dummies" : $"{dummyIndex} dummy")}";
                    return true;
            }
        }
    }
}
*/
