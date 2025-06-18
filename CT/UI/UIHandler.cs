using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SFDCT.Misc;
using SFD;
using SFD.MenuControls;
using HarmonyLib;
using SFD.Effects;
using SFDCT.Configuration;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class UIHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Menu), nameof(Menu.Area), MethodType.Getter)]
    private static void GetMenuArea(ref Rectangle __result, Menu __instance)
    {
        __result.Inflate(10, 10);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScrollBar), nameof(ScrollBar.MouseClick))]
    private static bool ScrollBarMouseClick(ScrollBar __instance, Rectangle mouseSelection)
    {
        int handleCenter = __instance.handlePosition + __instance.handle.Height / 2;

        if (mouseSelection.Y > handleCenter)
        {
            while (mouseSelection.Y > handleCenter)
            {
                __instance.parentMenu.Scroll(1);

                handleCenter = __instance.handlePosition + __instance.handle.Height / 2;
                if (__instance.parentMenu.topItemId == 0 || __instance.parentMenu.bottomItemId == __instance.parentMenu.VisibleItems.Count - 1) break;
            }
        }
        else if (mouseSelection.Y < handleCenter)
        {
            while (mouseSelection.Y < handleCenter)
            {
                __instance.parentMenu.Scroll(-1);

                handleCenter = __instance.handlePosition + __instance.handle.Height / 2;
                if (__instance.parentMenu.topItemId == 0 || __instance.parentMenu.bottomItemId == __instance.parentMenu.VisibleItems.Count - 1) break;
            }
        }

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(74).operand = $"{Constants.VERSION} - {Globals.Version.SFDCT}";
        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FilmGrain), nameof(FilmGrain.Draw))]
    private static bool FilmGrainDraw()
    {
        if (Settings.Get<bool>(SettingKey.HideFilmgrain))
        {
            return false;
        }

        return true;
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
