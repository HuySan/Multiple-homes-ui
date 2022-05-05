using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Steamworks;
using MultipleHomesUI.Controllers;
using MultipleHomesUI.HomeSettings;
using UnityEngine;
using System.Collections;
using Rocket.Unturned.Events;
using Rocket.API.Collections;
using Logger = Rocket.Core.Logging.Logger;

namespace MultipleHomesUI
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin instance;
        private const string _CloseUi = "CloseUi";
        private const string _CloseRenameUi = "CloseRenameUi";
        private const string _BtnRename = "BtnRename";
        public Dictionary<CSteamID, Dictionary<string, DateTime>> cooldowns = new Dictionary<CSteamID, Dictionary<string, DateTime>>();
        private RenameController _renameController;

        protected override void Load()
        {
            instance = this;
            _renameController = new RenameController();
            base.Load();

            if (Configuration.Instance.downloadWorkshopHomedUi)
                WorkshopDownloadConfig.getOrLoad().File_IDs.Add(2798466786);
            if (Configuration.Instance.downloadWorkshopRenameUi)
                WorkshopDownloadConfig.getOrLoad().File_IDs.Add(2803593326);

            PlayerInput.onPluginKeyTick += KeyTickEvent;
            EffectManager.onEffectButtonClicked += ButtonClickerEvent;
            UnturnedPlayerEvents.OnPlayerUpdateGesture += PlayerUpdateGesture;
            EffectManager.onEffectTextCommitted += TextCommited;
            Logger.Log("###########################################", ConsoleColor.Magenta);
            Logger.Log("#    Plugin Created By fucking Lincoln    #", ConsoleColor.Magenta);
            Logger.Log("#          Plugin Version: 2.0.0          #", ConsoleColor.Magenta);
            Logger.Log("###########################################", ConsoleColor.Magenta);
            Logger.Log("MultipleHomedUi has been loaded!", ConsoleColor.Yellow);
            Logger.Log("");
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "destroyed_bed", "Некоторые спальники были уничтожены другими игроками или вами" },
            { "max_bed_exceeded", "Превышено кол-во допустимых спальников, поэтому последний спальник был удалён" },
            { "delay", "Вы будете телепортированы через {0} сек" },
            { "isDriving", "Нельзя телепортироваться, находясь в транспортном средстве" },
        };

        private void TextCommited(Player player, string buttonName, string text)
        {
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);
            if (buttonName == "InputField")
                _renameController.SendRenameText(ref uplayer, ref text);
        }
        
        private void PlayerUpdateGesture(UnturnedPlayer uplayer, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture != UnturnedPlayerEvents.PlayerGesture.Point)
                return;
            _renameController.CheckPointBed(ref uplayer, out _, true);
        }

        private void ButtonClickerEvent(Player player, string buttonName)
        {
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);

            if (buttonName.ToLower() == _CloseRenameUi.ToLower() || buttonName.ToLower() == _BtnRename.ToLower())
            {
                _renameController.CloseUiEffect(ref uplayer);
                return;
            }

            if(buttonName.ToLower() == _CloseUi.ToLower())
            {
                HomesManager.CloseUi(uplayer);
                return;
            }

            for (int i = 0; i <= 6; i++)
            {              
                if (buttonName.ToLower() == $"home{i}")
                {
                    if (CooldownController.IsColldown(uplayer, ref buttonName, out _))
                        return;
                    HomesManager.TeleportationToHome(uplayer, buttonName);
                }
            }

            for (int i = 0; i <= 6; i++)
            {
                if (buttonName == $"destroyHome{i}")
                    HomesManager.DeleteHome(uplayer, ref buttonName);
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
            PlayerInput.onPluginKeyTick -= KeyTickEvent;
            EffectManager.onEffectButtonClicked -= ButtonClickerEvent;
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= PlayerUpdateGesture;
            EffectManager.onEffectTextCommitted -= TextCommited;
        }
    }
}
