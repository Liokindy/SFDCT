using HarmonyLib;
using SFD.Effects;
using SFDCT.Configuration;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class FilmgrainHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FilmGrain), nameof(FilmGrain.Draw))]
    private static bool FilmGrainDraw()
    {
        if (Settings.Get<bool>(SettingKey.HideFilmgrain))
        {
            return false;
        }

        return true;
    }
}