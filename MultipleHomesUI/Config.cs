using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleHomesUI
{
    public class Config : IRocketPluginConfiguration
    {
        public int maxHomes;
        public ushort effectId;
        public short effectKey;
        public int cooldown;
        public int delay;
        public int setHomeId;
        public void LoadDefaults()
        {
            delay = 5;
            cooldown = 30;
            maxHomes = 7;
            effectId = 7394;
            effectKey = 7394;
            setHomeId = 288;
        }
    }
}
