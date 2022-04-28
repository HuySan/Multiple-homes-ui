﻿using Rocket.API;
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
        public ushort effectIdRenameBtn;
        public short effectKeyRenameBtn;
        public int cooldown;
        public int delay;
        public void LoadDefaults()
        {
            delay = 5;
            cooldown = 120;
            maxHomes = 7;
            effectId = 7394;
            effectKey = 7394;
            effectIdRenameBtn = 7494;
            effectKeyRenameBtn = 7494;
        }
    }
}
