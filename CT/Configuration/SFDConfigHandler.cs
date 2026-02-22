using HarmonyLib;
using SFD;

namespace SFDCT.Configuration;

[HarmonyPatch]
internal static class SFDConfigHandler
{
    internal const SFDConfigSaveMode SAVE_MODERATOR_COMMANDS = (SFDConfigSaveMode)10;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDConfig), nameof(SFDConfig.SaveConfig))]
    private static void SFDConfig_SaveConfig_Prefix_SaveExtraSettings(SFDConfigSaveMode mode)
    {
        // This is a Prefix patch so the actual saving of the file and
        // checking for exceptions is done by the vanilla SaveConfig method

        object saveConfigLock = SFDConfig.m_saveConfigLock;
        lock (saveConfigLock)
        {
            if (mode == SAVE_MODERATOR_COMMANDS || mode == SFDConfigSaveMode.All)
            {
                SFDConfig.ConfigHandler.UpdateValue("MODERATOR_COMMANDS", string.Join(" ", Constants.MODDERATOR_COMMANDS));
            }

            if (mode == SFDConfigSaveMode.Settings || mode == SFDConfigSaveMode.All)
            {
                SFDConfig.ConfigHandler.UpdateValue("PRIMARY_COLOR", Constants.COLORS.MENU_BLUE.ToHex());
                SFDConfig.ConfigHandler.UpdateValue("CLIENT_REQUEST_SERVER_MOVEMENT", Constants.CLIENT_REQUEST_SERVER_MOVEMENT);
            }
        }
    }
}
