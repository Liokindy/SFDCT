using System;
using System.Collections;
using System.Collections.Generic;
using SFD;
using HarmonyLib;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.Game;

/// <summary>
///     Potential bug-fix for users not being able to open maps in
///     vanilla-SFD, might be caused by maps being saved as v.1.3.7x 
/// </summary>
[HarmonyPatch]
internal static class VersionPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.WriteToStream))]
    private static IEnumerable<CodeInstruction> SFDMapEditorBuildTreeViewImageList(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction code in instructions)
        {
            if (code.operand == null)
            {
                continue;
            }
            if (code.operand.Equals("v.1.3.7x"))
            {
                code.operand = CConst.Version.SFD;
            }
        }
        return instructions;
    }
}
