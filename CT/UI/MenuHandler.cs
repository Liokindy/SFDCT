using HarmonyLib;
using SFD;
using SFD.MenuControls;
using SFDCT.Misc;
using SFDCT.UI.Panels;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class MenuHandler
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.KeyPress))]
    private static IEnumerable<CodeInstruction> MainMenuPanel_KeyPress_Transpiler_EscapeKeyCheck(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction?.opcode.Equals(OpCodes.Ldc_I4_7) == true)
            {
                instruction.opcode = OpCodes.Ldc_I4_8;
            }
        }

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> GameSFD_DrawInner_Transpiler_VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction?.operand?.Equals(Constants.VERSION) == true)
            {
                instruction.operand = $"{Constants.VERSION} - {Globals.Version.SFDCT}";
            }
        }

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanel_Constructor_Postfix_InsertSFDCTOption(MainMenuPanel __instance)
    {
        // if (!SFD.Program.IsGame) return;

        var menu = __instance.menu;
        var sfdctSettings = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object _) =>
        {
            __instance.OpenSubPanel(new SFDCTSettingsPanel());
        }));

        sfdctSettings.Initialize(menu);

        __instance.Height += 1;

        menu.Height += 1;
        menu.Items.Insert(Math.Max(menu.Items.Count - 2, 0), sfdctSettings);

        __instance.UpdatePosition();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenuPanel), MethodType.Constructor)]
    private static void GameMenuPanel_Constructor_Postfix_InsertSFDCTOption(GameMenuPanel __instance)
    {
        var menu = (Menu)__instance.members[0];
        var sfdctSettings = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object _) =>
        {
            __instance.OpenSubPanel(new SFDCTSettingsPanel());
        }));

        sfdctSettings.Initialize(menu);

        menu.Height += 1;

        menu.Items.Insert(menu.Items.Count - 1, sfdctSettings);
    }
}
