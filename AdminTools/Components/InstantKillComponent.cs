﻿using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using Handlers = Exiled.Events.Handlers;

namespace AdminTools
{
    public class InstantKillComponent : MonoBehaviour
    {
        public Player Player;
        public void Awake()
        {
            Player = Player.Get(gameObject);
            Handlers.Player.Hurting += RunWhenPlayerIsHurt;
            Handlers.Player.Left += OnLeave;
            Plugin.IkHubs.Add(Player, this);
        }

        private void OnLeave(LeftEventArgs ev)
        {
            if (ev.Player == Player)
                Destroy(this);
        }

        public void OnDestroy()
        {
            Handlers.Player.Hurting -= RunWhenPlayerIsHurt;
            Handlers.Player.Left -= OnLeave;
            Plugin.IkHubs.Remove(Player);
        }

        public void RunWhenPlayerIsHurt(HurtingEventArgs ev)
        {
            if (ev.Attacker != ev.Player && ev.Attacker == Player)
                ev.Amount = int.MaxValue;
        }
    }
}