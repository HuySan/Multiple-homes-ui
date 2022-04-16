using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleHomesUI.Controllers
{
    public static class CooldownController
    {
        public static bool IsColldown(this UnturnedPlayer uplayer, ref string homeId)
        {
            if (!Plugin.instance.cooldowns.ContainsKey(uplayer.CSteamID))
                return false;
            if (!Plugin.instance.cooldowns[uplayer.CSteamID].TryGetValue(homeId, out DateTime lastCalled))
                return false;

            int distinction = Plugin.instance.Configuration.Instance.cooldown - ((int)(DateTime.Now - lastCalled).TotalSeconds);
            if(distinction > 0)
            {
                var timeLeft = TimeSpan.FromSeconds(distinction);
                UnturnedChat.Say(uplayer, $"Вы снова сможете отправить тп на эту спалку через {timeLeft} сек");//Отображение в UI с отчётом
                //Делаем кнопку времени активной и показываем оставшееся кд.
                return true;
            }
            return false;
        }

        public static void SetCooldown(this UnturnedPlayer uplayer, string homeId)
        {
            Rocket.Core.Logging.Logger.Log("Вызван SETcooldown");
            if (Plugin.instance.cooldowns.ContainsKey(uplayer.CSteamID))
            {
                Plugin.instance.cooldowns[uplayer.CSteamID][homeId] = DateTime.Now;
                return;
            }

            Plugin.instance.cooldowns.Add(uplayer.CSteamID, new Dictionary<string, DateTime> { { homeId, DateTime.Now } });


        }
    }
}
