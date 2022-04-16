using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Plugins;
using MultipleHomesUI.HomeSettings;
using Newtonsoft.Json;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using Rocket.Unturned.Chat;

namespace MultipleHomesUI
{
    public static class HomesManager
    {
        private static ushort _effectId = Plugin.instance.Configuration.Instance.effectId;
        private static short _effectKey = Plugin.instance.Configuration.Instance.effectKey;

        public static void SetHome(this UnturnedPlayer uplayer)
        {
            if(GetPlayerHomeCount(uplayer) >= Plugin.instance.Configuration.Instance.maxHomes)
            {
                UnturnedChat.Say(uplayer, "Превышен лимит спалок");  
                return;
            }
            HomesController.CreateHome(uplayer, uplayer.Position);
        }

        static int GetPlayerHomeCount(this UnturnedPlayer uplayer)
        {
            if (HomesController.HomesList.Homes.ContainsKey(uplayer.CSteamID.m_SteamID))
            {
                return HomesController.HomesList.Homes[uplayer.CSteamID.m_SteamID].Count;
            }
            return 0;
        }

        public static void DeleteHome(this UnturnedPlayer uplayer, ref string destroyHomeId)
        {
            HomesController.DeletePlayerHome(uplayer, destroyHomeId);
            UpdateUi(uplayer);
        }

        //Вызывается когда юзер нажмёт на нужный спальник в ui
        public static void TeleportationToHome(this UnturnedPlayer uplayer, string name)
        {
            //Сделаем тут проверку существует ли спальник         
            HomesController.PlayerTeleportationToHome(uplayer, name);
        }
        
        private static void UpdateUi(this UnturnedPlayer uplayer)
        {
            EffectManager.askEffectClearByID(_effectId, uplayer.SteamPlayer().transportConnection);
            CallindUi(uplayer);
        }

        public static  void CallindUi(this UnturnedPlayer uplayer)
        {
            string[] HomesId = HomesController.GetPlayerHomes(uplayer);
            string[] Colors = HomesController.GetPlayerColors(uplayer);
            int i = 0;
            EffectManager.sendUIEffect(_effectId, _effectKey, uplayer.SteamPlayer().transportConnection, true);
            //Сделать активными спалки, которые есть у юзера
            foreach (var id in HomesId)
            {
                EffectManager.sendUIEffectVisibility(_effectKey, uplayer.SteamPlayer().transportConnection, true, id, true);                
            }

            foreach (var color in Colors)
            {
                EffectManager.sendUIEffectImageURL(_effectKey, uplayer.SteamPlayer().transportConnection, true, HomesController.GiveColor(i), $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/{color}.png");
                i++;
            }
        }
    }
}
