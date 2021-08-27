using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NorthwoodLib.Pools;
using System;
using System.Text;

namespace AdminTools.Commands.BreakDoors
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class BreakDoors : ParentCommand
    {
        public BreakDoors() => LoadGeneratedCommands();

        public override string Command { get; } = "breakdoors";

        public override string[] Aliases { get; } = new string[] { "bd" };

        public override string Description { get; } = "Manage breaking door/gate properties for players";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.bd"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nbreakdoors ((player id / name) or (all / *)) ((doors) or (all))" +
                    "\nbreakdoors clear" +
                    "\nbreakdoors list" +
                    "\nbreakdoors remove (player id / name)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: breakdoors clear";
                        return false;
                    }

                    foreach (Player ply in Plugin.BdHubs.Keys)
                        if (ply.ReferenceHub.TryGetComponent(out BreakDoorComponent bdCom))
                            UnityEngine.Object.Destroy(bdCom);

                    response = "Breaking doors has been removed from everyone";
                    return true;
                case "list":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: breakdoors list";
                        return false;
                    }

                    StringBuilder playerLister = StringBuilderPool.Shared.Rent(Plugin.BdHubs.Count != 0 ? "Players with break doors on:\n" : "No players currently online have breaking doors on");
                    if (Plugin.BdHubs.Count == 0)
                    {
                        response = playerLister.ToString();
                        return true;
                    }

                    foreach (Player ply in Plugin.BdHubs.Keys)
                    {
                        playerLister.Append(ply.Nickname);
                        playerLister.Append(", ");
                    }

                    string msg = playerLister.ToString().Substring(0, playerLister.ToString().Length - 2);
                    StringBuilderPool.Shared.Return(playerLister);
                    response = msg;
                    return true;
                case "remove":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors remove (player id / name)";
                        return false;
                    }

                    Player pl = Player.Get(arguments.At(1));
                    if (pl == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (pl.ReferenceHub.TryGetComponent(out BreakDoorComponent bdComponent))
                    {
                        Plugin.BdHubs.Remove(pl);
                        UnityEngine.Object.Destroy(bdComponent);
                        response = $"Breaking doors is off for {pl.Nickname}";
                    }
                    else
                        response = $"Player {pl.Nickname} does not have the ability to break doors";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors (all / *) ((doors) or (all))";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out BreakType type))
                    {
                        response = $"Invalid breaking type: {arguments.At(1)}";
                        return false;
                    }

                    foreach (Player ply in Player.List)
                    {
                        if (!ply.ReferenceHub.TryGetComponent(out BreakDoorComponent bdCom))
                        {
                            ply.GameObject.AddComponent<BreakDoorComponent>();
                            switch (type)
                            {
                                case BreakType.Doors:
                                    ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = false;
                                    ply.IsBypassModeEnabled = false;
                                    break;
                                case BreakType.All:
                                    ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = true;
                                    ply.IsBypassModeEnabled = true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (type)
                            {
                                case BreakType.Doors:
                                    bdCom.breakAll = false;
                                    ply.IsBypassModeEnabled = false;
                                    break;
                                case BreakType.All:
                                    bdCom.breakAll = true;
                                    ply.IsBypassModeEnabled = true;
                                    break;
                            }
                        }
                    }

                    response = $"Breaking {((type == BreakType.Doors) ? "doors" : "everything")} is on for everyone now";
                    return true;
                default:
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors (player id / name) ((doors) or (all))";
                        return false;
                    }

                    Player plyr = Player.Get(arguments.At(0));
                    if (plyr == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out BreakType T))
                    {
                        response = $"Invalid breaking type: {arguments.At(1)}";
                        return false;
                    }

                    if (!plyr.ReferenceHub.TryGetComponent(out BreakDoorComponent bdComp))
                    {
                        plyr.GameObject.AddComponent<BreakDoorComponent>();
                        switch (T)
                        {
                            case BreakType.Doors:
                                plyr.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = false;
                                plyr.IsBypassModeEnabled = false;
                                break;
                            case BreakType.All:
                                plyr.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = true;
                                plyr.IsBypassModeEnabled = true;
                                break;
                        }

                        response = $"Breaking {((T == BreakType.Doors) ? "doors" : "all")} is on for {plyr.Nickname}";
                    }
                    else
                    {
                        switch (T)
                        {
                            case BreakType.Doors:
                                bdComp.breakAll = false;
                                plyr.IsBypassModeEnabled = false;
                                break;
                            case BreakType.All:
                                bdComp.breakAll = true;
                                plyr.IsBypassModeEnabled = true;
                                break;
                        }

                        response = $"Breaking {((T == BreakType.Doors) ? "doors" : "all")} is on for {plyr.Nickname}";
                    }
                    return true;
            }
        }
    }
}
