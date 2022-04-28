using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.Core.Plugins;
using MultipleHomesUI.HomeSettings;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;

namespace MultipleHomesUI.Patches
{
    public class TryCheckPhysicHome
    {
        HomesList home = HomesController.HomesList;

        List<Vector3> _interactableBedsPosition = new List<Vector3>();
        List<Vector3> _homesPosition = new List<Vector3>();

        private void _AddHomesPositions(ref UnturnedPlayer uplayer)
        {
            if (!home.homes.ContainsKey(uplayer.CSteamID.m_SteamID))
                return;

            foreach(BarricadeRegion region in BarricadeManager.regions)
            {
                foreach(BarricadeDrop drop in region.drops)
                {
                    InteractableBed interactable = drop.interactable as InteractableBed;

                    if (interactable != null && drop.GetServersideData().owner == uplayer.CSteamID.m_SteamID)//null
                    {
                        _interactableBedsPosition.Add(interactable.transform.position);
                    }
                }
            }
            
            foreach(var home in home.homes[uplayer.CSteamID.m_SteamID])
            {
                _homesPosition.Add(home.position);
            }
        }

        public bool CompaireHomesPositions(UnturnedPlayer uplayer) 
        {
            _AddHomesPositions(ref uplayer);

            List<Vector3> diffirent = _homesPosition.Except(_interactableBedsPosition).ToList();//Позиция удалённых спальников физически

            if (diffirent.Count == 0)
                return false;

            foreach (Vector3 position in diffirent)
            {
                foreach(var homesListItem in home.homes[uplayer.CSteamID.m_SteamID])
                {
                    if(position == homesListItem.position)
                    {
                        HomesController.DeletePlayerHome(uplayer, "destroy" + homesListItem.homeId, true);
                       //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Plugin.instance.cooldowns[uplayer.CSteamID].Remove(homesListItem.homeId);
                        return true;                 
                    }
                }
            }
            return true;
        }

    }
}
