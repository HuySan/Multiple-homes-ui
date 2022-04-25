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
using MultipleHomesUI.Patches;

namespace MultipleHomesUI
{
    public static class HomesManager
    {
        private static ushort _effectId = Plugin.instance.Configuration.Instance.effectId;
        private static short _effectKey = Plugin.instance.Configuration.Instance.effectKey;

        public static void DeleteHome(this UnturnedPlayer uplayer, ref string destroyHomeId)
        {
            HomesController.DeletePlayerHome(uplayer, destroyHomeId, false);
            UpdateUi(uplayer);
        }

        //Вызывается когда юзер нажмёт на иконку спальника
        public static void TeleportationToHome(this UnturnedPlayer uplayer, string name)
        {    
            HomesController.PlayerTeleportationToHome(uplayer, name);
        }
        
        private static void UpdateUi(this UnturnedPlayer uplayer)
        {
            EffectManager.askEffectClearByID(_effectId, uplayer.SteamPlayer().transportConnection);
            CallindUi(uplayer);
        }

        public static void CloseUi(this UnturnedPlayer uplayer)
        {
            EffectManager.askEffectClearByID(_effectId, uplayer.SteamPlayer().transportConnection);
            ShowDefaultUi(uplayer.Player);
        }

        public static void HideDefaultUi(this Player player)
        {
            player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Default);
            
        }

        public static void ShowDefaultUi(this Player player)
        {
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            player.enablePluginWidgetFlag(EPluginWidgetFlags.Default);
            
        }

        public static  void CallindUi(this UnturnedPlayer uplayer)
        {
            string[] homesId = HomesController.GetPlayerHomes(uplayer);
            string[] colors = HomesController.GetPlayerColors(uplayer);
            TryCheckPhysicHome checkHome = new TryCheckPhysicHome();
            int i = 0;

            HideDefaultUi(uplayer.Player);

            //Удаляем спальники из словаря, которые были уничтожены физическим путём
            if (checkHome.CompaireHomesPositions(uplayer))
                UnturnedChat.Say(uplayer, "Некоторые спальники были уничтожены другими игроками или вами");


            EffectManager.sendUIEffect(_effectId, _effectKey, uplayer.SteamPlayer().transportConnection, true);

            //Делаем иконки спалок активными, которые есть у юзера
            foreach (var id in homesId)
                EffectManager.sendUIEffectVisibility(_effectKey, uplayer.SteamPlayer().transportConnection, true, id, true);                

            foreach (var color in colors)
            {
                EffectManager.sendUIEffectImageURL(_effectKey, uplayer.SteamPlayer().transportConnection, true, "Home" + i, $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/{color}.png");
                i++;
            }

            HomesController.ShowAvailableHomes(uplayer);
        }
    }
}
