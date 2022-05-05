using SDG.Unturned;

namespace MultipleHomesUI.Patches
{
    public class HideShowDefaultUi
    {
        public void HideDefaultUi(Player player)
        {
            player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Default);

        }

        public void ShowDefaultUi(Player player)
        {
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            player.enablePluginWidgetFlag(EPluginWidgetFlags.Default);
        }
    }
}
