using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFD.MenuControls;
using HarmonyLib;
using SFDCT.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SFDCT.UI;

/// <summary>
///     Patches the Panel and ScrollBar classes to give scroll bars functionality again.
/// </summary>
[HarmonyPatch]
internal static class ScrollHandler
{
    // We use a slightly larger area for elements when checking for mouse stuff,
    // this way the edges of the elements can be clicked properly.

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Panel), nameof(Panel.MouseClick))]
    private static bool PanelMouseClick(Panel __instance, Rectangle mouseSelection)
    {
        if (__instance.subPanel != null)
        {
            __instance.subPanel.MouseClick(mouseSelection);
            return false;
        }

        for (int i = 0; i < __instance.members.Count; i++)
        {
            Rectangle memberRect = GetElementArea(__instance.members[i].Area);

            if (mouseSelection.Intersects(memberRect))
            {
                __instance.SelectMember(i);
                __instance.members[i].MouseClick(mouseSelection);
                return false;
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Panel), nameof(Panel.MouseMove))]
    private static bool PanelMouseMove(Panel __instance, Rectangle currentSelection, Rectangle previousSelection)
    {
        if (__instance.subPanel != null)
        {
            __instance.subPanel.MouseMove(currentSelection, previousSelection);
            return false;
        }
        for (int i = 0; i < __instance.members.Count; i++)
        {
            Rectangle memberRect = GetElementArea(__instance.members[i].Area);

            if (currentSelection.Intersects(memberRect))
            {
                __instance.SelectMember(i);
                __instance.members[i].MouseMove(currentSelection, previousSelection);
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Panel), nameof(Panel.MouseDown))]
    private static bool PanelMouseDown(Panel __instance, Rectangle mouseSelection)
    {
        if (__instance.subPanel != null)
        {
            __instance.subPanel.MouseDown(mouseSelection);
            return false;
        }
        for (int i = 0; i < __instance.members.Count; i++)
        {
            Rectangle memberRect = GetElementArea(__instance.members[i].Area);

            if (mouseSelection.Intersects(memberRect))
            {
                __instance.members[i].MouseDown(mouseSelection);
            }
        }
        return false;
    }
    
    /// <summary>
    ///     This makes it possible to click the edges of menus,
    ///     making scrollbars click-eable again.
    /// </summary>
    private static Rectangle GetElementArea(Rectangle elementArea)
    {
        elementArea.Inflate(12, 12);
        return elementArea;
    }

    /// <summary>
    ///     The scroll bar uses a while statement to scroll towards the mouse. It can get stucked
    ///     if the mouse clicks near the bottom/top as it will try to go lower/higher but the actual
    ///     position wont change.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScrollBar), nameof(ScrollBar.MouseClick))]
    private static bool ScrollBarMouseClick(ScrollBar __instance, Rectangle mouseSelection)
    {
        if (mouseSelection.Y > __instance.handlePosition)
        {
            int lastHandlePosition = __instance.handlePosition;
            while (mouseSelection.Y > __instance.handlePosition)
            {
                __instance.parentMenu.Scroll(1);
                if (lastHandlePosition != __instance.handlePosition)
                {
                    lastHandlePosition = __instance.handlePosition;
                }
                else { break; }
            }
            return false;
        }
        else if (mouseSelection.Y < __instance.handlePosition)
        {
            int lastHandlePosition = __instance.handlePosition;
            while (mouseSelection.Y < __instance.handlePosition)
            {
                __instance.parentMenu.Scroll(-1);
                if (lastHandlePosition != __instance.handlePosition)
                {
                    lastHandlePosition = __instance.handlePosition;
                }
                else { break; }
            }
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScrollBar), nameof(ScrollBar.Area), MethodType.Getter)]
    private static void ScrollBarGetArea(ref Rectangle __result)
    {
        __result.Width = 10;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScrollBar), nameof(ScrollBar.handle), MethodType.Getter)]
    private static void ScrollBarGetHandle(ref Rectangle __result)
    {
        __result.Width = 10;
    }
}
