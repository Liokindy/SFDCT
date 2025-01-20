using System;
using System.Linq;
using SFD.MenuControls;
using HarmonyLib;

namespace SFDCT.UI.Panels.Tweaks
{
    [HarmonyPatch]
    internal class LobbySlotDropdown
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LobbySlotDropdownPanel), MethodType.Constructor, new Type[] { typeof(LobbySlot), typeof(MenuItemButton[]) })]
        private static void LobbySlotDropdownPanelConstructor(LobbySlotDropdownPanel __instance, params MenuItemButton[] items)
        {
            int newHeight = __instance.Height;
            if (items != null)
            {
                newHeight = items.Length * Menu.ITEM_HEIGHT;
            }

            __instance.Height = newHeight;
            __instance.members.First().Height = newHeight;
        }
    }
}
