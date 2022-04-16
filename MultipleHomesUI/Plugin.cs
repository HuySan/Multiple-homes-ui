using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using MultipleHomesUI.Controllers;
using MultipleHomesUI.HomeSettings;

namespace MultipleHomesUI
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin instance;
        private const string _CreateHome = "ButtonCreate";
        public Dictionary<CSteamID, Dictionary<string, DateTime>> cooldowns = new Dictionary<CSteamID, Dictionary<string, DateTime>>();

        protected override void Load()
        {
            instance = this;
            base.Load();
            PlayerInput.onPluginKeyTick += KeyTickEvent;
            EffectManager.onEffectButtonClicked += ButtonClickerEvent;

        }

        private void ButtonClickerEvent(Player player, string buttonName)
        {
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);

            if(buttonName.ToLower() == _CreateHome.ToLower())
            {
                HomesManager.SetHome(uplayer);
                return;
            }

            for (int i = 0; i <= 7; i++)
            {              
                if (buttonName.ToLower() == $"home{i}")
                {
                    if (CooldownController.IsColldown(uplayer, ref buttonName))
                        return;
                    HomesManager.TeleportationToHome(uplayer, buttonName);
                }
            }

            for (int i = 0; i <= 7; i++)//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!У нас спалки от 0 до 6
            {
                if (buttonName == $"destroyHome{i}")
                {
                    Rocket.Core.Logging.Logger.Log("Вошли в условие destroyHome{i}");
                    HomesManager.DeleteHome(uplayer, ref buttonName);
                }
            }
        }

        private void KeyTickEvent(Player player, uint simulation, byte key, bool state)
        {
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);

            if (player.input.keys[11])//"/slash"
            {
                HomesManager.CallindUi(uplayer);
            }
        }

        protected override void Unload()
        {
            base.Unload();
            instance = null;
        }
    }
}
