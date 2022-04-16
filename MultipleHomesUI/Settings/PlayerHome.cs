using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultipleHomesUI.HomeSettings
{
    public class PlayerHome
    {
        public string homeId;
        public Vector3 positionPlayer;
        public string color;

        public PlayerHome(string homeId, Vector3 positionPlayer, string color)
        {
            this.homeId = homeId;
            this.positionPlayer = positionPlayer;
            this.color = color;
        }
        public PlayerHome() { }

    }
}
