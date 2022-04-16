using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Plugins;
using MultipleHomesUI.HomeSettings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using System.Runtime.Serialization;
using MultipleHomesUI.Controllers;
using Rocket.Unturned.Chat;
using System.Collections;

namespace MultipleHomesUI
{
    public static class HomesController
    {
        private static string _homesDirectory => Path.Combine(Plugin.instance.Directory, "homes.json");
        private static HomesList _homesList;
        private static int _delay = Plugin.instance.Configuration.Instance.delay;

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
        

        public static void  CreateHome(this UnturnedPlayer uplayer, Vector3 position)
        {
            string homeId;
            int homeCount;
            
            if (HomesList.Homes.ContainsKey(uplayer.CSteamID.m_SteamID))
            {
                homeCount = HomesList.Homes[uplayer.CSteamID.m_SteamID].Count;
                homeId = "Home" + homeCount;
                HomesList.Homes[uplayer.CSteamID.m_SteamID].Add(new PlayerHome(homeId, position, GiveColor(homeCount)));
                EffectManager.sendUIEffectVisibility(Plugin.instance.Configuration.Instance.effectKey, uplayer.SteamPlayer().transportConnection, true, homeId, true);
                EffectManager.sendUIEffectImageURL(Plugin.instance.Configuration.Instance.effectKey, uplayer.SteamPlayer().transportConnection, true, GiveColor(homeCount), $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/{GiveColor(homeCount)}.png");
            }
            else
            {
                HomesList.Homes.Add(uplayer.CSteamID.m_SteamID, new List<PlayerHome> { new PlayerHome("Home0", position, "Blue") });
                EffectManager.sendUIEffectVisibility(Plugin.instance.Configuration.Instance.effectKey, uplayer.SteamPlayer().transportConnection, true, "Home0", true);
                EffectManager.sendUIEffectImageURL(Plugin.instance.Configuration.Instance.effectKey, uplayer.SteamPlayer().transportConnection, true, "Blue", $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/Blue.png");
            }

            //Так же создаём спалку физическую, если есть необходимое кол-во ткани, иначе return
            SetPhysicalHome(uplayer);
            Save();            
        }

        public static void SetPhysicalHome(this UnturnedPlayer uplayer)
        {
            var barricade = new Barricade((ItemBarricadeAsset)Assets.find(EAssetType.ITEM, 288));
            if (BarricadeManager.tryGetPlant(uplayer.Player.transform, out byte _, out byte _, out ushort _,  out BarricadeRegion region))
                BarricadeManager.dropNonPlantedBarricade(barricade, uplayer.Position, Quaternion.Euler(-90f, uplayer.Rotation, 0f), uplayer.CSteamID.m_SteamID, uplayer.Player.quests.groupID.m_SteamID);
        }

        //Будет возвращать цвет спалки взависимости от кол-ва спалок в homes
        public static string GiveColor(int homesCount)
        {
            List<string> Color = new List<string>() {"Blue", "Red", "Green", "Yellow", "Purple", "Orange", "Cyan"};
            return Color[homesCount];
        }

        public static void DeletePlayerHome(this UnturnedPlayer player, string destroyHomeId)
        {           
            //заполняем брешь
            foreach(var id in HomesList.Homes[player.CSteamID.m_SteamID])
            {
                if (int.Parse(destroyHomeId.Substring(11)) < int.Parse(id.homeId.Substring(4)))
                {
                    var temp = int.Parse(id.homeId.Substring(4)) - 1;
                    id.homeId = "Home" + temp.ToString();
                }
            }

            var itemForRemove = HomesList.Homes[player.CSteamID.m_SteamID].FirstOrDefault(r => r.homeId.ToLower() == destroyHomeId.Substring(7).ToLower());
            HomesList.Homes[player.CSteamID.m_SteamID].Remove(itemForRemove);//удаляем спальник по ид
            Save();
        }

        public static  string[] GetPlayerHomes(this UnturnedPlayer uplayer)
        {
            if (HomesList.Homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return HomesList.Homes[uplayer.CSteamID.m_SteamID].Select(x => x.homeId).ToArray();
            return new string[0];
        }

        public static string[] GetPlayerColors(this UnturnedPlayer uplayer)
        {
            if (HomesList.Homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return HomesList.Homes[uplayer.CSteamID.m_SteamID].Select(x => x.color).ToArray();

            return new string[0];
        }

        public static void PlayerTeleportationToHome(this UnturnedPlayer uplayer, string homeId)
        {             
            if (_delay != 0)
                UnturnedChat.Say(uplayer, $"Вы будете телепортированы через {_delay} сек");
            Plugin.instance.StartCoroutine(GoHome(uplayer, homeId));           
        }

        private static IEnumerator GoHome(this UnturnedPlayer uplayer,  string homeId)
        {
            yield return new WaitForSeconds(_delay);
            
            if (HomesList.Homes[uplayer.CSteamID.m_SteamID].Exists(x => x.homeId == homeId))
            {
                var res = HomesList.Homes[uplayer.CSteamID.m_SteamID].Where(x => x.homeId == homeId).ToList();
                Vector3 homePosition = res[0].positionPlayer;
                uplayer.Player.teleportToLocation(homePosition, uplayer.Rotation);
                CooldownController.SetCooldown(uplayer, homeId);
            }
        }

        public static void Save()
        {
            Rocket.Core.Logging.Logger.Log("Сохранили");
            File.WriteAllText(_homesDirectory, JsonConvert.SerializeObject(HomesList, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }
    }
}
