using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
using CConst = SFDCT.Misc.Globals;
using CSettings = SFDCT.Settings.Values;
using HarmonyLib;
using SFDCT.Helper;

namespace SFDCT.UI;

/// <summary>
///     Patches the visuals of the main menu
/// </summary>
[HarmonyPatch]
internal static class MainMenuHandler
{
    private static Panel mainMenuPanel = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), MethodType.Constructor)]
    private static void MainMenuPanelConstructor(MainMenuPanel __instance)
    {
        if (SFD.Program.IsGame)
        {
            mainMenuPanel = __instance;

            Menu menu = ((Menu)__instance.members[0]);
            MainMenuItem sfdctSettings = new MainMenuItem("SFDCT", new ControlEvents.ChooseEvent((object obj) => { __instance.OpenSubPanel(new Panels.SFDCTSettingsPanel()); }));

            sfdctSettings.Initialize(menu);

            menu.Height += 32;
            menu.Items.Insert(menu.Items.Count - 1, sfdctSettings);
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

        menu.Height += 32;
        menu.Items.Insert(menu.Items.Count - 1, sfdctSettings);
    }

    /// <summary>
    ///     The texture that covers the left side of the main menu is drawn using 
    ///     Black as a color. Changing to white allows the user to use a custom texture
    ///     without it looking black.
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Draw))]
    private static IEnumerable<CodeInstruction> MainMenu_PatchBg(IEnumerable<CodeInstruction> instructions)
    {
        for (int i = 5; i <= 22; i++)
        {
            instructions.ElementAt(i).opcode = OpCodes.Nop;
            instructions.ElementAt(i).operand = null;
        }
        return instructions;
    }
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameMenuPanel), nameof(GameMenuPanel.Draw))]
    private static IEnumerable<CodeInstruction> GameMenu_PatchBg(IEnumerable<CodeInstruction> instructions)
    {
        for (int i = 3; i <= 20; i++)
        {
            instructions.ElementAt(i).opcode = OpCodes.Nop;
            instructions.ElementAt(i).operand = null;
        }
        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.Draw))]
    private static void MainMenu_DrawBg(SpriteBatch batch, MainMenuPanel __instance)
    {
        DrawMainMenuBG(batch, __instance.mainMenuBG);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameMenuPanel), nameof(GameMenuPanel.Draw))]
    private static void GameMenuPanel_DrawBg(SpriteBatch batch, GameMenuPanel __instance)
    {
        DrawMainMenuBG(batch, __instance.mainMenuBG);
    }

    public static void DrawMainMenuBG(SpriteBatch sb, Texture2D mainMenuBG)
    {
        if (mainMenuBG != null)
        {
            Color drawColor = CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuBgUseBlack)) ? Color.Black : Color.White;
            int scale = 2;

            for (int i = 0; i < GameSFD.GAME_HEIGHT + mainMenuBG.Height * scale; i += mainMenuBG.Height * scale)
            {
                sb.Draw(mainMenuBG, new Rectangle(0, i, mainMenuBG.Width * scale, mainMenuBG.Height * scale), drawColor);
            }
        }
    }

    /// <summary>
    ///     Change the label at the corner showing the game's version
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(76).operand = $"{CConst.Version.SFD} - {CConst.Version.LABEL}";
        return instructions;
    }
}
