﻿using System.Collections.Generic;
using System.Linq;
using SFD;
using SFD.MenuControls;
using SFDCT.Misc;
using HarmonyLib;

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
        foreach (var instruction in instructions)
        {
            if (instruction != null && instruction.operand != null && instruction.operand.Equals(Constants.VERSION))
            {
                instruction.operand = $"{Constants.VERSION} - {Globals.Version.SFDCT}";
            }
        }

        return instructions;
    }

    private static Panel mainMenuPanel = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanelConstructor(MainMenuPanel __instance)
    {
        if (SFD.Program.IsGame)
        {
            mainMenuPanel = __instance;

            Menu menu = __instance.menu;
            MainMenuItem sfdctSettings = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object obj) => { __instance.OpenSubPanel(new Panels.SFDCTSettingsPanel()); }));

            sfdctSettings.Initialize(menu);

            __instance.Height += 1;
            menu.Height += 1;
            menu.Items.Insert(menu.Items.Count - 2, sfdctSettings);
            __instance.UpdatePosition();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenuPanel), MethodType.Constructor)]
    private static void GameMenuPanelConstructor(GameMenuPanel __instance)
    {
        mainMenuPanel = __instance;

        Menu menu = ((Menu)__instance.members[0]);
        MainMenuItem sfdctSettings = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object obj) => { __instance.OpenSubPanel(new Panels.SFDCTSettingsPanel()); }));

        sfdctSettings.Initialize(menu);

        menu.Height += 1;
        menu.Items.Insert(menu.Items.Count - 1, sfdctSettings);
    }
}
