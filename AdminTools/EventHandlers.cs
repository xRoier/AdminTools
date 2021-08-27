using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions;
using Interactables.Interobjects;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using RemoteAdmin;
using UnityEngine;
using Log = Exiled.API.Features.Log;
using Object = UnityEngine.Object;

namespace AdminTools
{
	using Exiled.API.Extensions;
	using Exiled.API.Features.Items;
	using InventorySystem.Items.Firearms.Attachments;
	using InventorySystem.Items.ThrowableProjectiles;
	using Ragdoll = Exiled.API.Features.Ragdoll;

	public class EventHandlers
	{
		private readonly Plugin _plugin;
		public EventHandlers(Plugin plugin) => this._plugin = plugin;

		public void OnDoorOpen(InteractingDoorEventArgs ev)
		{
			if (Plugin.PryGateHubs.Contains(ev.Player))
				ev.Door.TryPryOpen();
		}

		public static string FormatArguments(ArraySegment<string> sentence, int index)
		{
			StringBuilder sb = StringBuilderPool.Shared.Rent();
			foreach (string word in sentence.Segment(index))
			{
				sb.Append(word);
				sb.Append(" ");
			}
			string msg = sb.ToString();
			StringBuilderPool.Shared.Return(sb);
			return msg;
		}

		public static void SpawnDummyModel(Player ply, Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z, out int dummyIndex)
		{
			dummyIndex = 0;
			GameObject obj =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
			if (ccm == null)
				Log.Error("CCM is null, this can cause problems!");
			ccm.CurClass = role;
			ccm.GodMode = true;
			obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(x, y, z);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
			if (Plugin.DumHubs.TryGetValue(ply, out List<GameObject> objs))
			{
				objs.Add(obj);
			}
			else
			{
				Plugin.DumHubs.Add(ply, new List<GameObject>());
				Plugin.DumHubs[ply].Add(obj);
				dummyIndex = Plugin.DumHubs[ply].Count();
			}
			if (dummyIndex != 1)
				dummyIndex = objs.Count();
		}

		public static IEnumerator<float> SpawnBodies(Player player, RoleType role, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Ragdoll.Spawn(role, DamageTypes.Falldown, "SCP-343", player.Position, default, default, false, 0);
				yield return Timing.WaitForSeconds(0.15f);
			}
		}

        public void OnPlayerDestroyed(DestroyingEventArgs ev)
        {
			if (Plugin.RoundStartMutes.Contains(ev.Player))
            {
				ev.Player.IsMuted = false;
				Plugin.RoundStartMutes.Remove(ev.Player);
            }
        }

        public static void SpawnWorkbench(Player ply, Vector3 position, Vector3 rotation, Vector3 size, out int benchIndex)
		{
			benchIndex = 0;
			GameObject bench =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
			rotation.x += 180;
			rotation.z += 180;
			Offset offset = new Offset();
			offset.position = position;
			offset.rotation = rotation;
			offset.scale = Vector3.one;
			bench.gameObject.transform.localScale = size;
			NetworkServer.Spawn(bench);
			if (Plugin.BchHubs.TryGetValue(ply, out List<GameObject> objs))
			{
				objs.Add(bench);
			}
			else
			{
				Plugin.BchHubs.Add(ply, new List<GameObject>());
				Plugin.BchHubs[ply].Add(bench);
				benchIndex = Plugin.BchHubs[ply].Count();
			}
			if (benchIndex != 1)
				benchIndex = objs.Count();
			bench.transform.localPosition = offset.position;
			bench.transform.localRotation = Quaternion.Euler(offset.rotation);
			bench.AddComponent<WorkstationController>();
		}

		public static void SpawnItem(ItemType type, Vector3 pos, Vector3 rot)
		{
			new Item(type).Spawn(pos, Quaternion.Euler(rot));
		}

		public static void SetPlayerScale(GameObject target, float x, float y, float z)
		{
			try
			{
				NetworkIdentity identity = target.GetComponent<NetworkIdentity>();
				target.transform.localScale = new Vector3(1 * x, 1 * y, 1 * z);

				ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
				destroyMessage.netId = identity.netId;

				foreach (GameObject player in PlayerManager.players)
				{
					NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;
					if (player != target)
						playerCon.Send(destroyMessage, 0);

					object[] parameters = new object[] { identity, playerCon };
					Extensions.InvokeStaticMethod(typeof(NetworkServer), "SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Log.Info($"Set Scale error: {e}");
			}
		}

		public static void SetPlayerScale(GameObject target, float scale)
		{
			try
			{
				NetworkIdentity identity = target.GetComponent<NetworkIdentity>();
				target.transform.localScale = Vector3.one * scale;

				ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
				destroyMessage.netId = identity.netId;

				foreach (GameObject player in PlayerManager.players)
				{
					if (player == target)
						continue;

					NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;
					playerCon.Send(destroyMessage, 0);

					object[] parameters = new object[] { identity, playerCon };
					Extensions.InvokeStaticMethod(typeof(NetworkServer), "SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Log.Info($"Set Scale error: {e}");
			}
		}

		public static IEnumerator<float> DoRocket(Player player, float speed)
		{
			const int maxAmnt = 50;
			int amnt = 0;
			while (player.Role != RoleType.Spectator)
			{
				player.Position += Vector3.up * speed;
				amnt++;
				if (amnt >= maxAmnt)
				{
					player.IsGodModeEnabled = false;
					SpawnGrenadeOnPlayer(player, GrenadeType.Frag, 0.05f);
					player.Kill();
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		// I found the below code to be very overcomplicated for the task at hand. Not sure why it was written like this......

		/*
		public static IEnumerator<float> DoTut(Player player)
		{
			if (player.IsOverwatchEnabled)
				player.IsOverwatchEnabled = false;

			player.Role = RoleType.Tutorial;
			yield return Timing.WaitForSeconds(1f);
			Door[] d = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door in d)
				if (door.DoorName == "SURFACE_GATE")
				{
					player.Position = door.transform.position + Vector3.up * 2;
					break;
				}

			player.ReferenceHub.serverRoles.CallTargetSetNoclipReady(player.ReferenceHub.characterClassManager.connectionToClient, true);
			player.ReferenceHub.serverRoles.NoclipReady = true;
		}*/

		public static IEnumerator<float> DoJail(Player player, bool skipadd = false)
		{
			List<Item> items = new List<Item>();
			Dictionary<AmmoType, ushort> ammo = new Dictionary<AmmoType, ushort>();
			foreach(AmmoType type in (AmmoType []) Enum.GetValues(typeof(AmmoType)))
			{
				ammo[type] = player.Ammo[ItemExtensions.GetItemType(type)];
			}
			foreach (Item item in player.Items)
				items.Add(item);
			if (!skipadd)
				Plugin.JailedPlayers.Add(new Jailed
				{
					Health = player.Health,
					Position = player.Position,
					Items = items,
					Name = player.Nickname,
					Role = player.Role,
					Userid = player.UserId,
					CurrentRound = true,
					Ammo = ammo
				});
			if (player.IsOverwatchEnabled)
				player.IsOverwatchEnabled = false;
			yield return Timing.WaitForSeconds(1f);
			player.Role = RoleType.Tutorial;
			player.Position = new Vector3(53f, 1020f, -44f);
			player.ClearInventory();
		}

		public static IEnumerator<float> DoUnJail(Player player)
		{
			Jailed jail = Plugin.JailedPlayers.Find(j => j.Userid == player.UserId);
			if (jail.CurrentRound)
			{
				player.Role = jail.Role;
				player.ResetInventory(jail.Items);
				yield return Timing.WaitForSeconds(1.5f);
				player.Health = jail.Health;
				player.Position = jail.Position;
				foreach (AmmoType type in (AmmoType[])Enum.GetValues(typeof(AmmoType)))
				{
					player.Ammo[type.GetItemType()] = jail.Ammo[type];
				}
			}
			else
			{
				player.Role = RoleType.Spectator;
			}
			Plugin.JailedPlayers.Remove(jail);
		}

		public static void SpawnGrenadeOnPlayer(Player player, GrenadeType type, float timer)
		{
			Vector3 spawnrand = player.Position + new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
			EffectGrenade grenade;
			switch (type)
			{
				case GrenadeType.Flash:
					grenade = (EffectGrenade) new FlashGrenade(ItemType.GrenadeFlash){FuseTime = timer}.Spawn(spawnrand).Base;
					break;
				default:
					grenade = (EffectGrenade) new ExplosiveGrenade(type == GrenadeType.Scp018
						? ItemType.SCP018
						: ItemType.GrenadeHE){FuseTime = timer}.Spawn(spawnrand).Base;
					break;
			}

			grenade.ServerActivate();
		}

		public void OnPlayerVerified(VerifiedEventArgs ev)
		{
			try
			{
				if (Plugin.JailedPlayers.Any(j => j.Userid == ev.Player.UserId))
					Timing.RunCoroutine(DoJail(ev.Player, true));

				if (File.ReadAllText(_plugin.OverwatchFilePath).Contains(ev.Player.UserId))
				{
					Log.Debug($"Putting {ev.Player.UserId} into overwatch.");
					Timing.CallDelayed(1, () => ev.Player.IsOverwatchEnabled = true);
				}

				if (File.ReadAllText(_plugin.HiddenTagsFilePath).Contains(ev.Player.UserId))
				{
					Log.Debug($"Hiding {ev.Player.UserId}'s tag.");
					Timing.CallDelayed(1, () => ev.Player.BadgeHidden = true);
				}

				if (Plugin.RoundStartMutes.Count != 0 && !ev.Player.ReferenceHub.serverRoles.RemoteAdmin && !Plugin.RoundStartMutes.Contains(ev.Player))
                {
					Log.Debug($"Muting {ev.Player.UserId} (no RA).");
					ev.Player.IsMuted = true;
					Plugin.RoundStartMutes.Add(ev.Player);
                }
			}
			catch (Exception e)
			{
				Log.Error($"Player Join: {e}");
			}
		}

		public void OnRoundStart()
		{
			foreach (Player ply in Plugin.RoundStartMutes)
			{
				if (ply != null)
				{
					ply.IsMuted = false;
				}
			}
			Plugin.RoundStartMutes.Clear();
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			try
			{
				List<string> overwatchRead = File.ReadAllLines(_plugin.OverwatchFilePath).ToList();
				List<string> tagsRead = File.ReadAllLines(_plugin.HiddenTagsFilePath).ToList();

				foreach (Player player in Player.List)
				{
					string userId = player.UserId;

					if (player.IsOverwatchEnabled && !overwatchRead.Contains(userId))
						overwatchRead.Add(userId);
					else if (!player.IsOverwatchEnabled && overwatchRead.Contains(userId))
						overwatchRead.Remove(userId);

					if (player.BadgeHidden && !tagsRead.Contains(userId))
						tagsRead.Add(userId);
					else if (!player.BadgeHidden && tagsRead.Contains(userId))
						tagsRead.Remove(userId);
				}

				foreach (string s in overwatchRead)
					Log.Debug($"{s} is in overwatch.");
				foreach (string s in tagsRead)
					Log.Debug($"{s} has their tag hidden.");
				File.WriteAllLines(_plugin.OverwatchFilePath, overwatchRead);
				File.WriteAllLines(_plugin.HiddenTagsFilePath, tagsRead);

				// Update all the jails that it is no longer the current round, so when they are unjailed they don't teleport into the void.
				foreach (Jailed jail in Plugin.JailedPlayers)
				{
					if(jail.CurrentRound)
						jail.CurrentRound = false;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Round End: {e}");
			}

			if (Plugin.RestartOnEnd)
			{
				Log.Info("Restarting server....");
				Round.Restart(false, true, ServerStatic.NextRoundAction.Restart);
			}
		}

		public void OnTriggerTesla(TriggeringTeslaEventArgs ev)
		{
			if (ev.Player.IsGodModeEnabled)
				ev.IsTriggerable = false;
		}

		public void OnSetClass(ChangingRoleEventArgs ev)
		{
			if (_plugin.Config.GodTuts)
				ev.Player.IsGodModeEnabled = ev.NewRole == RoleType.Tutorial;
		}

		public void OnWaitingForPlayers()
		{
			Plugin.IkHubs.Clear();
			Plugin.BdHubs.Clear();
		}
	}
}
