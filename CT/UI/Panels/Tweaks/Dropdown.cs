//using System;
//using SFD;
//using SFD.MenuControls;
//using HarmonyLib;

//namespace SFDCT.UI.Panels.Tweaks
//{
//    [HarmonyPatch]
//    internal static class Dropdown
//    {
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MenuItemDropdown), MethodType.Constructor, new Type[] { typeof(string), typeof(string[]) })]
//        private static void MenuItemDropdownConstructor(MenuItemDropdown __instance)
//        {
//            if (__instance.values != null && __instance.values.Length > 0)
//            {
//                int length = __instance.values.Length;
//                float maxHeight = __instance.ParentMenu != null ? __instance.ParentMenu.Height : GameSFD.GAME_HEIGHT * 0.4f;

//                while (length * Menu.ITEM_HEIGHT > maxHeight && length > 3)
//                {
//                    length--;
//                }
                
//                __instance.DropdownItemVisibleCount = length;
//            }
//        }
//    }
//}
