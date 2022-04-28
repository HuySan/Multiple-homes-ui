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
using UnityEngine;
using System.Collections;

namespace MultipleHomesUI
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin instance;
        private const string _CloseUi = "CloseUi";
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
            
            if(buttonName.ToLower() == _CloseUi.ToLower())
            {
                HomesManager.CloseUi(uplayer);
                return;
            }

            for (int i = 0; i <= 7; i++)
            {              
                if (buttonName.ToLower() == $"home{i}")
                {
                    if (CooldownController.IsColldown(uplayer, ref buttonName, out _))
                        return;
                    HomesManager.TeleportationToHome(uplayer, buttonName);
                }
            }

            for (int i = 0; i <= 7; i++)
            {
                if (buttonName == $"destroyHome{i}")
                {
                    HomesManager.DeleteHome(uplayer, ref buttonName);
                    foreach(string homeId in cooldowns[uplayer.CSteamID].Keys)
                    {
                        Rocket.Core.Logging.Logger.Log("Ид спалки в cooldowns " + homeId);
                    }
                }
            }
        }

        private void KeyTickEvent(Player player, uint simulation, byte key, bool state)
        {
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);

            if (player.input.keys[11])//"/slash"
                HomesManager.CallindUi(uplayer);

        }


        protected override void Unload()
        {
            base.Unload();
            instance = null;
        }
    }
}
