using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SFD;
using Constants = SFR.Misc.Constants;

namespace SFR.UI;

/// <summary>
///     Overrides the version label on the bottom left corner
///     of the screen
/// </summary>
[HarmonyPatch]
internal static class VersionLabel
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> DrawInner(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(76).operand = Constants.SFRVersion + " - " + Constants.ClientVersion;
        return code;
    }
}
