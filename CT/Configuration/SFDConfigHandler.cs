using HarmonyLib;
using SFD;

namespace SFDCT.Configuration;

[HarmonyPatch]
internal static class SFDConfigHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDConfig), nameof(SFDConfig.SaveConfig))]
    private static void SFDConfig_SaveConfig_Prefix_SaveExtraSettings(SFDConfigSaveMode mode)
    {
        object saveConfigLock = SFDConfig.m_saveConfigLock;
        lock (saveConfigLock)
        {
            if (mode == SFDConfigSaveMode.Settings)
            {
                SFDConfig.ConfigHandler.UpdateValue("PRIMARY_COLOR", Constants.COLORS.MENU_BLUE.ToHex());
            }
        }

        // This is a Prefix patch so the actual saving of the file and
        // checking for exceptions is done by the vanilla SaveConfig method
    }
}
