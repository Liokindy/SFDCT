using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
//using CConst = SFDCT.Misc.Globals;
//using CSettings = SFDCT.Settings.Values;
using HarmonyLib;
using SFDCT.Helper;

namespace SFDCT.UI;

/// <summary>
///     Patches the visuals of the main menu
/// </summary>
[HarmonyPatch]
internal static class MainMenuHandler
{
    /// <summary>
    ///     Change the label at the corner showing the game's version
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(74).operand = $"{SFD.Constants.VERSION} - v.1.0.6-beta.4";
        return instructions;
    }
}
