using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SFD.MapEditor;
using SFD.States;

namespace SFDCT.Editor;

[HarmonyPatch]
internal static class FormTweaks
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateEditor), nameof(StateEditor.Load))]
    private static void PatchEditorWindowIcon(StateEditor __instance)
    {
        // Fix the editor window displaying SFD icon
        __instance.m_mapEditorForm.Icon = Program.GameIcon;
    }
}