using System;
using System.Collections.Generic;
using System.Linq;
using SFD;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SFR.Game;
/// <summary>
///     This class contains patches that affect GameSFD
/// </summary>
[HarmonyPatch]
internal static class Game
{
    private static Color ColorFromHue(float hue)
    {
        int hi = (int)(Math.Floor(hue * 0.016666666f) % 6);
        float f = hue * 0.016666666f - (float)Math.Floor(hue * 0.016666666f);

        int q = (int)((1 - f) * 255);
        int t = (int)((1 - (1 - f)) * 255);
        if (hi == 0)
            return new Color(255, t, 0);
        else if (hi == 1)
            return new Color(q, 255, 0);
        else if (hi == 2)
            return new Color(0, 255, t);
        else if (hi == 3)
            return new Color(0, q, 255);
        else if (hi == 4)
            return new Color(t, 0, 255);
        else
            return new Color(255, 0, q);
    }

}