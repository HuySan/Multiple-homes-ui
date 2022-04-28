using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultipleHomesUI.Controllers
{
    public static class CooldownController
    {
        public static bool IsColldown(this UnturnedPlayer uplayer, ref string homeId, out string timeLeft)
        {
            timeLeft = null;

            if (!Plugin.instance.cooldowns.ContainsKey(uplayer.CSteamID))
                return false;
            if (!Plugin.instance.cooldowns[uplayer.CSteamID].TryGetValue(homeId, out DateTime lastCalled))
                return false;

            int distinction = Plugin.instance.Configuration.Instance.cooldown - ((int)(DateTime.Now - lastCalled).TotalSeconds);
            if(distinction > 0)
            {
                timeLeft = TimeSpan.FromSeconds(distinction).TotalSeconds.ToString();
                // UnturnedChat.Say(uplayer, $"Вы снова сможете отправить тп на эту спалку через {timeLeft} сек");
                return true;
            }
            return false;
        }

        public static void SetCooldown(this UnturnedPlayer uplayer, string homeId)
        {
            if (Plugin.instance.cooldowns.ContainsKey(uplayer.CSteamID))
            {
                Plugin.instance.cooldowns[uplayer.CSteamID][homeId] = DateTime.Now;
                SetStub(uplayer, homeId);
                return;
            }

            Plugin.instance.cooldowns.Add(uplayer.CSteamID, new Dictionary<string, DateTime> { { homeId, DateTime.Now } });
            SetStub(uplayer, homeId);
        }

        private static void SetStub(this UnturnedPlayer uplayer, string homeId)
        {
            string stubTime = $"StubTime{homeId.Substring(4)}";
            string timeCount = $"TimeCount{homeId.Substring(4)}";

            if (!IsColldown(uplayer, ref homeId, out string timeLeft))
                return;

            Plugin.instance.StartCoroutine(Timer(uplayer, timeCount, timeLeft, stubTime));
        }

        private static IEnumerator Timer(this UnturnedPlayer uplayer, string timeCount, string timeLeft, string stubTime)
        {
            short key = Plugin.instance.Configuration.Instance.effectKey;
            while (true)
            {
                if (uplayer.SteamPlayer() != null) 
                { 
                    EffectManager.sendUIEffectVisibility(key, uplayer.SteamPlayer().transportConnection, true, stubTime, true);
                    EffectManager.sendUIEffectText(key, uplayer.SteamPlayer().transportConnection, true, timeCount, timeLeft);
                }

                timeLeft = (int.Parse(timeLeft) - 1).ToString();
                yield return new WaitForSeconds(1);

                if (int.Parse(timeLeft) <= 0)
                {
                    if (uplayer.SteamPlayer() != null)
                        EffectManager.sendUIEffectVisibility(key, uplayer.SteamPlayer().transportConnection, true, stubTime, false);
                    yield break;
                }
            }
        }
    }
}
