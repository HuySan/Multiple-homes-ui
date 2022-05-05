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
        private static HideShowDefaultUi _hideShowDefaultUi = new HideShowDefaultUi();

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
            _hideShowDefaultUi.ShowDefaultUi(uplayer.Player);
        }


        public static  void CallindUi(this UnturnedPlayer uplayer)
        {
            string[] homesId = HomesController.GetPlayerHomes(uplayer);
            string[] colors = HomesController.GetPlayerColors(uplayer);
            string[] names = HomesController.GetHomesName(uplayer);
            TryCheckPhysicHome checkHome = new TryCheckPhysicHome();
            int i = 0;

            _hideShowDefaultUi.HideDefaultUi(uplayer.Player);

            //Удаляем спальники из словаря, которые были уничтожены физическим путём
            if (checkHome.CompaireHomesPositions(uplayer))
                UnturnedChat.Say(uplayer, Plugin.instance.Translate("destroyed_bed"), Color.yellow);


            EffectManager.sendUIEffect(_effectId, _effectKey, uplayer.SteamPlayer().transportConnection, true);

            //Делаем иконки спалок активными, которые есть у юзера
            foreach (var id in homesId)
                EffectManager.sendUIEffectVisibility(_effectKey, uplayer.SteamPlayer().transportConnection, true, id, true);                

            foreach (var color in colors)
            {
                EffectManager.sendUIEffectImageURL(_effectKey, uplayer.SteamPlayer().transportConnection, true, "Home" + i, $"https://raw.githubusercontent.com/HuySan/Homes-Color-png/main/{color}.png");
                i++;
            }

            i = 0;
            foreach(var name in names)
            {
                EffectManager.sendUIEffectText(_effectKey, uplayer.SteamPlayer().transportConnection, true, "NameBed" + i, name.ToUpper());
                i++;
            }

            HomesController.ShowAvailableHomes(uplayer, out _);
        }
    }
}
