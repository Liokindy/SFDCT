using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD;
using HarmonyLib;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class SlowmotionHandler
{
    // Keep for future use
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.SlowmotionHandler), nameof(SFD.SlowmotionHandler.UpdatePlayerSlowmotions))]
    private static bool UpdatePlayerSlowmotions(SFD.SlowmotionHandler __instance)
    {
        for (int i = 0; i < __instance.GameWorld.Players.Count; i++)
        {
            __instance.GameWorld.Players[i].SlowmotionFactor = 1f;
        }
        for (int j = 0; j < __instance.m_slowmotions.Count; j++)
        {
            Slowmotion slowmotion = __instance.m_slowmotions[j];
            if (slowmotion.PlayerOwnerID != 0)
            {
                Player player = __instance.GameWorld.GetPlayer(slowmotion.PlayerOwnerID);
                if (player != null)
                {
                    float currentIntensity = slowmotion.GetCurrentIntensity();
                    player.SlowmotionFactor = 1f / (1f - (1f - currentIntensity) * 1f); // * 0.25f
                    player.SlowmotionProjectileFactor = 1f / (1f - (1f - currentIntensity) * 1f); // * 0.5f
                }
            }
        }

        return false;
    }
    */
}
