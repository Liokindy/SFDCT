using HarmonyLib;
using SFD.Effects;
using SFDCT.Configuration;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class FilmgrainHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FilmGrain), nameof(FilmGrain.Draw))]
    private static bool FilmGrain_Draw_Prefix_CheckHide()
    {
        return !SFDCTConfig.Get<bool>(CTSettingKey.HideFilmgrain);
    }
}
