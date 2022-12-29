using CommandSystem;
using Exiled.Permissions.Extensions;
using Mirror;
using System;
using Exiled.API.Features;

namespace AdminTools.Commands.Cleanup
{
    class Ragdolls : ICommand
    {
        public string Command { get; } = "ragdolls";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Cleans up ragdolls on the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("at.cleanup"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: cleanup ragdolls";
                return false;
            }

            
            foreach (Ragdoll doll in Map.Ragdolls)
                NetworkServer.Destroy(doll.GameObject);

            response = "Ragdolls have been cleaned up now";
            return true;
        }
    }
}
