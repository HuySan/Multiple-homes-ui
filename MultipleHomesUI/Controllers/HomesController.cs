using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MultipleHomesUI.HomeSettings;
using Newtonsoft.Json;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using MultipleHomesUI.Controllers;
using Rocket.Unturned.Chat;
using System.Collections;
using MultipleHomesUI.Patches;

namespace MultipleHomesUI
{
    public static class HomesController
    {
        private static string _homesDirectory => Path.Combine(Plugin.instance.Directory, "homes.json");
        private static HomesList _homesList;
        private static int _delay = Plugin.instance.Configuration.Instance.delay;
        private static short _key = Plugin.instance.Configuration.Instance.effectKey;

        public static HomesList HomesList//Возвращает значение словаря с homes
        {
            get
            {
                if (_homesList == null)
                {
                    if (File.Exists(_homesDirectory))
                    {
                        _homesList = JsonConvert.DeserializeObject<HomesList>(File.ReadAllText(_homesDirectory), new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    }
                    else
                    {
                        _homesList = new HomesList();
                    }
                }
                return _homesList;
            }
        }
        
        public static void ShowAvailableHomes(this UnturnedPlayer uplayer, out bool isDestroyed)
        {
            int homeCount;
            string homeId;
            int bedCount = 0;
            isDestroyed = false;

            foreach (BarricadeRegion region in BarricadeManager.regions)
            {
                foreach(BarricadeDrop drop in region.drops)
                {
                   InteractableBed interactableBed = drop.interactable as InteractableBed;

                    if (interactableBed == null)
                        continue;

                    if (drop.GetServersideData().owner != uplayer.CSteamID.m_SteamID)                   
                        continue;

                    bedCount++;

                    if (bedCount > Plugin.instance.Configuration.Instance.maxHomes)
                    {
                        DestroyPhysicalHome(interactableBed);
                        UnturnedChat.Say(uplayer, Plugin.instance.Translate("max_bed_exceeded"), Color.red);
                        isDestroyed = true;
                        return;
                    }


                    if (HomesList.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                    {
                        if (HomesList.homes[uplayer.CSteamID.m_SteamID].Exists(x => x.position == interactableBed.transform.position))
                            continue;

                        homeCount = HomesList.homes[uplayer.CSteamID.m_SteamID].Count;
                        homeId = "Home" + homeCount;

                        HomesList.homes[uplayer.CSteamID.m_SteamID].Add(new PlayerHome(homeId, interactableBed.transform.position, GiveColor(homeCount), "UNNAMED BED"));
                        EffectManager.sendUIEffectVisibility(_key, uplayer.SteamPlayer().transportConnection, true, homeId, true);
                        EffectManager.sendUIEffectImageURL(_key, uplayer.SteamPlayer().transportConnection, true, homeId, $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/{GiveColor(homeCount)}.png");
                    }
                    else
                    {
                        HomesList.homes.Add(uplayer.CSteamID.m_SteamID, new List<PlayerHome> { new PlayerHome("Home0", interactableBed.transform.position, "Blue", "UNNAMED BED") });
                        EffectManager.sendUIEffectVisibility(_key, uplayer.SteamPlayer().transportConnection, true, "Home0", true);
                        EffectManager.sendUIEffectImageURL(_key, uplayer.SteamPlayer().transportConnection, true, "Home0", $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/Blue.png");
                    }
                    Save();
                }
            }
        }

        //Будет возвращать цвет спалки взависимости от кол-ва спалок в homes
        public static string GiveColor(int homesCount)
        {
            List<string> Color = new List<string>() {"Blue", "Red", "Green", "Yellow", "Purple", "Orange", "Cyan"};
            return Color[homesCount];
        }

        public static void DeletePlayerHome(this UnturnedPlayer uplayer, string destroyHomeId, bool isDestroyer)
        {
            //заполняем брешь
            foreach (var id in HomesList.homes[uplayer.CSteamID.m_SteamID])
            {
                if (int.Parse(destroyHomeId.Substring(11)) < int.Parse(id.homeId.Substring(4)))
                {
                    var temp = int.Parse(id.homeId.Substring(4)) - 1;
                    id.homeId = "Home" + temp.ToString();
                }
            }

            var idForRemove = HomesList.homes[uplayer.CSteamID.m_SteamID].FirstOrDefault(r => r.homeId.ToLower() == destroyHomeId.Substring(7).ToLower());
            var positionForRemove = HomesList.homes[uplayer.CSteamID.m_SteamID].FirstOrDefault(r => r.position == idForRemove.position);

            if (!isDestroyer)
                DestroyPhysicalHomeByPosition(positionForRemove.position);
            HomesList.homes[uplayer.CSteamID.m_SteamID].Remove(idForRemove);
            Save();
        }

        public static void DestroyPhysicalHomeByPosition(Vector3 position)
        {
            foreach (BarricadeRegion region in BarricadeManager.regions)
            {
                foreach (BarricadeDrop drop in region.drops)
                {
                    InteractableBed interactableBed = drop.interactable as InteractableBed;
                    if (interactableBed == null)
                        continue;
                    if (interactableBed.transform.position != position)
                        continue;

                    if (!BarricadeManager.tryGetRegion(interactableBed.transform, out byte x, out byte y, out ushort plant, out _))
                        return;

                    BarricadeManager.destroyBarricade(drop, x, y, plant);
                    return;
                }
            }
        }   

        public static void DestroyPhysicalHome(InteractableBed interactableBed)
        {
            BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(interactableBed.transform);
            if (drop == null)
                return;

            if (!BarricadeManager.tryGetRegion(interactableBed.transform, out byte x, out byte y, out ushort plant, out _))
                return;

            BarricadeManager.destroyBarricade(drop, x, y, plant);
        }

        public static  string[] GetPlayerHomes(this UnturnedPlayer uplayer)
        {
            if (HomesList.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return HomesList.homes[uplayer.CSteamID.m_SteamID].Select(x => x.homeId).ToArray();
            return new string[0];
        }

        public static string[] GetHomesName(this UnturnedPlayer uplayer)
        {
            if (HomesList.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return HomesList.homes[uplayer.CSteamID.m_SteamID].Select(x => x.name).ToArray();
            return new string[0];
        }

        public static string[] GetPlayerColors(this UnturnedPlayer uplayer)
        {
            if (HomesList.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return HomesList.homes[uplayer.CSteamID.m_SteamID].Select(x => x.color).ToArray();

            return new string[0];
        }

        public static void PlayerTeleportationToHome(this UnturnedPlayer uplayer, string homeId)
        {             
            if (_delay != 0)
                UnturnedChat.Say(uplayer, Plugin.instance.Translate("delay", _delay), Color.yellow);
            CooldownController.SetCooldown(uplayer, homeId);
            Plugin.instance.StartCoroutine(GoHome(uplayer, homeId));           
        }

        private static bool ValidateTeleportation(this UnturnedPlayer uplayer, string homeId)
        {
            if (uplayer.Stance == EPlayerStance.DRIVING)
            {
                UnturnedChat.Say(uplayer, Plugin.instance.Translate("isDriving"), Color.red);
                return false;
            }

            if (uplayer.SteamPlayer() == null)
                return false;

            return true;
        }


        private static IEnumerator GoHome(this UnturnedPlayer uplayer,  string homeId)
        {
            yield return new WaitForSeconds(_delay);

            if (!ValidateTeleportation(uplayer, homeId))
                yield break;

            if (HomesList.homes[uplayer.CSteamID.m_SteamID].Exists(x => x.homeId == homeId))
            {
                var res = HomesList.homes[uplayer.CSteamID.m_SteamID].Where(x => x.homeId == homeId).ToList();
                Vector3 homePosition = res[0].position;
                try
                {
                    uplayer.Player.teleportToLocationUnsafe(homePosition, uplayer.Rotation);
                }
                catch(Exception ex)
                {
                    Rocket.Core.Logging.Logger.LogException(ex, "teleportation exception in coroutine 'GoHome'");
                }
            }
        }

        public static void Save()
        {
            File.WriteAllText(_homesDirectory, JsonConvert.SerializeObject(HomesList, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }
    }
}
