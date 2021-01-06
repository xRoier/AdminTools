using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
using UnityEngine;
using Handlers = Exiled.Events.Handlers;

namespace AdminTools
{
    public class BreakDoorComponent : MonoBehaviour
    {
        public Player player;
        public bool breakAll = false;
        string[] unbreakableDoorNames = { "079_FIRST", "079_SECOND", "372", "914", "CHECKPOINT_ENT", "CHECKPOINT_LCZ_A", "CHECKPOINT_LCZ_B", "GATE_A", "GATE_B", "SURFACE_GATE" };

        public void Awake()
        {
            Handlers.Player.InteractingDoor += OnDoorInteract;
            Handlers.Player.Left += OnLeave;
            player = Player.Get(gameObject);
            Plugin.BdHubs.Add(player, this);
        }

        private void OnLeave(LeftEventArgs ev)
        {
            if (ev.Player == player)
                Destroy(this);
        }

        private void OnDoorInteract(InteractingDoorEventArgs ev)
        {
            if (ev.Player != player)
                return;

            if (ev.Door is IDamageableDoor damageabledoor)
                BreakDoor(damageabledoor);
        }

        private void BreakDoor(IDamageableDoor door)
        {
            door.ServerDamage(ushort.MaxValue, DoorDamageType.ServerCommand);
        }

        public void OnDestroy()
        {
            Handlers.Player.InteractingDoor -= OnDoorInteract;
            Handlers.Player.Left -= OnLeave;
            player.IsBypassModeEnabled = false;
            breakAll = false;
            Plugin.BdHubs.Remove(player);
        }
    }
}