using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SFD.MenuControls;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class SetupPanel
{
    /// <summary>
    ///     Refresh config.ini after closing the settings panel.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.MenuControls.SetupPanel), nameof(SFD.MenuControls.SetupPanel.Dispose))]
    private static void RefreshConfigIni()
    {
        Misc.ConfigIni.Refresh();
    }
}
