using Microsoft.Xna.Framework;
using System;

namespace SFDCT.Helper;

internal static class ColorHelper
{
    internal static Color HSVToColor(float h, float s, float v)
    {
        var c = v * s;
        var x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
        var m = v - c;

        var rPrime = 0f;
        var gPrime = 0f;
        var bPrime = 0f;

        if (h < 60) { rPrime = c; gPrime = x; }
        else if (h < 120) { rPrime = x; gPrime = c; }
        else if (h < 180) { gPrime = c; bPrime = x; }
        else if (h < 240) { gPrime = x; bPrime = c; }
        else if (h < 300) { rPrime = x; bPrime = c; }
        else { rPrime = c; bPrime = x; }

        var r = (byte)((rPrime + m) * 255);
        var g = (byte)((gPrime + m) * 255);
        var b = (byte)((bPrime + m) * 255);

        return new Color(r, g, b);
    }
}
