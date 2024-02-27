using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.UI;

/// <summary>
///     Patches the labels shown on the corner of the screen.
///     i.e: version, account, etc
/// </summary>
[HarmonyPatch]
internal static class CornerLabelHandler
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(76).operand = $"{CConst.Version.SFD} - {CConst.Version.Label}";
        return instructions;
    }
}
