using SFD.Effects;
using CSettings = SFDCT.Settings.Values;
using HarmonyLib;

namespace SFDCT.UI
{
    [HarmonyPatch]
    internal static class FilmgrainHandler
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FilmGrain), nameof(FilmGrain.Draw))]
        private static bool FilmGrainDraw()
        {
            if (CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.HideFilmgrain)))
            {
                return false;
            }

            return true;
        }
    }
}
