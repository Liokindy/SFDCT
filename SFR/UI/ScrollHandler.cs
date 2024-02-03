using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFD.MenuControls;
using HarmonyLib;
using SFDCT.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Text;

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
        // Logger.LogDebug(mouseSelection.ToString());
        if (__instance.subPanel != null)
        {
            __instance.subPanel.MouseClick(mouseSelection);
            return false;
        }

        for (int i = 0; i < __instance.members.Count; i++)
        {
            Rectangle memberArea = __instance.members[i].Area;
            if (__instance.members[i] is Menu)
            {
                if (((Menu)__instance.members[i]).showScrollBar)
                {
                    memberArea = GetElementArea(__instance.members[i].Area);
                }
            }

            if (mouseSelection.Intersects(memberArea))
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
            Rectangle memberArea = __instance.members[i].Area;
            if (__instance.members[i] is Menu)
            {
                if (((Menu)__instance.members[i]).showScrollBar)
                {
                    memberArea = GetElementArea(__instance.members[i].Area);
                }
            }

            if (currentSelection.Intersects(memberArea))
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
            Rectangle memberArea = __instance.members[i].Area;
            if (__instance.members[i] is Menu)
            {
                if (((Menu)__instance.members[i]).showScrollBar)
                {
                    memberArea = GetElementArea(__instance.members[i].Area);
                }
            }

            if (mouseSelection.Intersects(memberArea))
            {
                __instance.members[i].MouseDown(mouseSelection);
            }
        }
        return false;
    }

    /// <summary>
    ///     If the user clicks near the scrollbar the game thinks it's outside
    ///     the dropdown panel and closes it. This patches that.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DropdownPanel), nameof(DropdownPanel.MouseClick))]
    private static bool DropdownPanelMouseClick(DropdownPanel __instance, Rectangle mouseSelection)
    {
        Rectangle memberRect = GetElementArea(__instance.Area);
        if (!mouseSelection.Intersects(memberRect))
        {
            __instance.Close();
            return false;
        }

        ScrollHandler.PanelMouseClick(__instance, mouseSelection);
        return false;
    }

    /// <summary>
    ///     This makes it possible to click the edges of menus,
    ///     making scrollbars click-eable again.
    /// </summary>
    private static Rectangle GetElementArea(Rectangle area)
    {
        Rectangle elementArea = area;
        elementArea.Inflate(14, 14);
        return elementArea;
    }

    /// <summary>
    ///     The scroll bar uses a while statement to scroll towards the mouse. It can get stucked
    ///     if the mouse clicks near the bottom/top as it will try to go lower/higher but the actual
    ///     position wont change. Causing an infinite loop and crashing the game.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScrollBar), nameof(ScrollBar.MouseClick))]
    private static bool ScrollBarMouseClick(ScrollBar __instance, Rectangle mouseSelection)
    {
        if (mouseSelection.Y > HandleCenter(__instance.handlePosition))
        {
            int lastHandlePosition = HandleCenter(__instance.handlePosition);
            while (mouseSelection.Y > HandleCenter(__instance.handlePosition))
            {
                __instance.parentMenu.Scroll(1);
                if (lastHandlePosition != HandleCenter(__instance.handlePosition))
                {
                    lastHandlePosition = HandleCenter(__instance.handlePosition);
                }
                else { break; }
            }
            return false;
        }
        else if (mouseSelection.Y < HandleCenter(__instance.handlePosition))
        {
            int lastHandlePosition = HandleCenter(__instance.handlePosition);
            while (mouseSelection.Y < HandleCenter(__instance.handlePosition))
            {
                __instance.parentMenu.Scroll(-1);
                if (lastHandlePosition != HandleCenter(__instance.handlePosition))
                {
                    lastHandlePosition = HandleCenter(__instance.handlePosition);
                }
                else { break; }
            }
        }
        return false;
    }
    private static int HandleCenter(int positionY)
    {
        // Scrollbar's handle is 28 pixels high
        positionY += 14;
        return positionY;
    }
}
