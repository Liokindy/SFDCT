using System.Collections.Generic;
using SFD;
using HarmonyLib;

namespace SFR.UI;

[HarmonyPatch]
internal static class VersionLabel
{
    /// <summary>
    ///     Patches the corner text showing the
    ///     version of the game
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> PatchVersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        foreach( var instruction in instructions)
        {
            if (instruction.operand is null)
            {
                continue;
            }

            if (instruction.operand.Equals("v.1.3.7x"))
            {
                instruction.operand = Misc.Constants.SFRVersion + "*";
            }
        }
        return instructions;
    }
}
