using UnityEngine;

namespace MultipleHomesUI.HomeSettings
{
    public class PlayerHome
    {
        public string homeId;
        public Vector3 position;
        public string color;
        public string name;

        public PlayerHome(string homeId, Vector3 position, string color, string name)
        {
            this.homeId = homeId;
            this.position = position;
            this.color = color;
            this.name = name;
        }
        public PlayerHome() { }

    }
}
