//using System.Collections.Generic;
//using SFD.MenuControls;
//using HarmonyLib;

//namespace SFDCT.UI.Panels
//{
//    [HarmonyPatch]
//    internal static class HookHandler
//    {
//        internal static Dictionary<object, object> HookedEvents = [];

//        internal static void Hook(object item, object value)
//        {
//            if (!HookedEvents.ContainsKey(item))
//            {
//                HookedEvents.Add(item, null);
//            }
//            HookedEvents[item] = value;
//        }
//        internal static void DisposeHook(object item)
//        {
//            if (HookedEvents.ContainsKey(item))
//            {
//                HookedEvents[item] = null;
//                HookedEvents.Remove(item);
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(MenuItemDropdown), nameof(MenuItemDropdown.TriggerValueChangedEvent))]
//        private static void MenuItemDropdownTriggerValueChangedEvent(MenuItemDropdown __instance)
//        {
//            if (__instance != null && HookedEvents.ContainsKey(__instance))
//            {
//                if (HookedEvents[__instance] != null && HookedEvents[__instance] is MenuItemValueChangedEvent)
//                {
//                    ((MenuItemValueChangedEvent)HookedEvents[__instance]).Invoke(null);
//                }
//            }
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(TextValidationItem), nameof(TextValidationItem.InvokeTextValidation))]
//        private static void TextValidationItemInvokeTextValidation(TextValidationItem __instance, string textToValidate, TextValidationEventArgs e)
//        {
//            if (__instance != null && HookedEvents.ContainsKey(__instance))
//            {
//                if (HookedEvents[__instance] is TextValidationEvent)
//                {
//                    ((TextValidationEvent)HookedEvents[__instance]).Invoke(textToValidate, e);
//                }
//            }
//        }
//    }
//}
