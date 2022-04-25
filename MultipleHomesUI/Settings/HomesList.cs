using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleHomesUI.HomeSettings
{
    public class HomesList
    {
        public Dictionary<ulong,  List<PlayerHome>> homes = new Dictionary<ulong, List<PlayerHome>>();
    }
}
