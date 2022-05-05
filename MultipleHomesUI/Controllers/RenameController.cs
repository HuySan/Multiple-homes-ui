using System;
using System.Collections.Generic;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using Rocket.Unturned.Chat;
using MultipleHomesUI.Patches;
using MultipleHomesUI.HomeSettings;
using System.Linq;

namespace MultipleHomesUI.Controllers
{
    public class RenameController
    {
        private HideShowDefaultUi _hideShowDefaultUi = new HideShowDefaultUi();

        private short _effectKey = Plugin.instance.Configuration.Instance.effectKeyRenameBtn;
        private ushort _effectId = Plugin.instance.Configuration.Instance.effectIdRenameBtn;
        HomesList home = HomesController.HomesList;

        public void CheckPointBed(ref UnturnedPlayer uplayer, out Transform obj, bool callingUi)
        {
            Transform objectTransform = TraceRay(uplayer, 5f, RayMasks.BARRICADE_INTERACT);
            obj = null;
            TryCheckPhysicHome physicHome = new TryCheckPhysicHome();

            if (objectTransform == null)
                return;

            BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(objectTransform);
            var bed = objectTransform.GetComponent<InteractableBed>();
            if (drop == null)
                return;

            if (bed == null)
                return;

            if (drop.GetServersideData().owner != uplayer.CSteamID.m_SteamID)
                return;

            obj = objectTransform;

            if (callingUi)
            {
                if(physicHome.CompaireHomesPositions(uplayer))
                    UnturnedChat.Say(uplayer, Plugin.instance.Translate("destroyed_bed"), Color.yellow);

                HomesController.ShowAvailableHomes(uplayer, out bool isDestroyed);
                if (isDestroyed)
                    return;

                CallingRenameUi(uplayer);
            }
        }

        public void CloseUiEffect(ref UnturnedPlayer uplayer)
        {
            EffectManager.askEffectClearByID(_effectId, uplayer.SteamPlayer().transportConnection);
            _hideShowDefaultUi.ShowDefaultUi(uplayer.Player);
        }

        public bool checkHome(UnturnedPlayer uplayer, Vector3 pos)
        {
            if (home.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
            {
                if (home.homes[uplayer.CSteamID.m_SteamID].Exists(r => r.position == pos))
                    return false;
                return true;
            }

            return true;
        }

        public void CallingRenameUi(UnturnedPlayer uplayer)
        {
            EffectManager.sendUIEffect(_effectId, _effectKey, uplayer.SteamPlayer().transportConnection, true);
            _hideShowDefaultUi.HideDefaultUi(uplayer.Player);
        }

        public void SendRenameText(ref UnturnedPlayer uplayer, ref string nameBed)
        {
            CheckPointBed(ref uplayer, out Transform obj, false);
            string homeIdByPos = GetHomeIdByPosition(uplayer, obj.position);

            if (homeIdByPos == null)
                return;

            foreach (var id in home.homes[uplayer.CSteamID.m_SteamID])
            {
                if (id.homeId == homeIdByPos)
                {
                    id.name = nameBed;
                    HomesController.Save();
                    return;
                }
            }
        }

        private string GetHomeIdByPosition(UnturnedPlayer uplayer, Vector3 pos)
        {
            //сравниваем позицию параметра и спалки в словаре, если равны возвращаем homeid
            var posByPoint = home.homes[uplayer.CSteamID.m_SteamID].FirstOrDefault(r => r.position == pos);

            if (posByPoint == null)
                return null;

            var temp =  home.homes[uplayer.CSteamID.m_SteamID].FirstOrDefault(r => r.homeId == posByPoint.homeId);
            return temp.homeId;
        }

        public Transform TraceRay(UnturnedPlayer uplayer, float distance, int mask)
        {
            if (!Physics.Raycast(uplayer.Player.look.aim.position, uplayer.Player.look.aim.forward, out var hit, distance, mask))
                return null;
            Transform transform = hit.transform;
            return transform;
        }
    }
}
