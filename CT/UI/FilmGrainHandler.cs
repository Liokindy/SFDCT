using SFD.Effects;
using SFDCT.Configuration;
using HarmonyLib;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class FilmgrainHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FilmGrain), nameof(FilmGrain.Draw))]
    private static bool FilmGrainDraw()
    {
        if (Settings.Get<bool>(Settings.SettingKey.HideFilmgrain))
        {
            return false;
        }

        return true;
    }
}