﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HarmonyLib;
using SFD;
using SFD.MapEditor;
using SFD.States;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.Editor;

// [HarmonyPatch]
/// <summary>
///     This fix causes the game to randomly not boot, and get stucked while
///     patching. Will stay disabled until another solution is done.
/// </summary>
internal static class Form
{
    /// <summary>
    ///     The game checks the version written into the ImagesList.sfdx file,
    ///     if it's different it rebuilds it again. This makes the game build it
    ///     with the target SFD version (v.1.3.7d) instead of v.1.3.7x. So if the
    ///     user uses vanilla-SFD, the imagelist won't be re-built.
    /// </summary>
    /*
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SFDMapEditor), nameof(SFDMapEditor.BuildTreeViewImageList), new Type[] { })]
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
    */
}