using System;
using System.Collections.Generic;
using SFD.MenuControls;
using HarmonyLib;

namespace SFDCT.UI.Panels;

[HarmonyPatch]
internal static class HookHandler
{
    internal static Dictionary<object, object> HookedEvents = [];

    internal static void Hook(object item, object value)
    {
        if (!HookedEvents.ContainsKey(item))
        {
            HookedEvents.Add(item, null);
        }
        HookedEvents[item] = value;
    }
    internal static void DisposeHook(object item)
    {
        if (HookedEvents.ContainsKey(item))
        {
            HookedEvents[item] = null;
            HookedEvents.Remove(item);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuItemSlider), nameof(MenuItemSlider.SetValue))]
    private static void MenuItemSliderSetValue(MenuItemSlider __instance, int value)
    {
        if (value != __instance.Value)
        {
            if (__instance != null && HookedEvents.ContainsKey(__instance))
            {
                if (HookedEvents[__instance] != null && HookedEvents[__instance] is Action)
                {
                    int oldValue = __instance.Value;
                    __instance.Value = value;
                    ((Action)HookedEvents[__instance]).Invoke();

                    __instance.Value = oldValue;
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuItemDropdown), nameof(MenuItemDropdown.TriggerValueChangedEvent))]
    private static void MenuItemDropdownTriggerValueChangedEvent(MenuItemDropdown __instance)
    {
        if (__instance != null && HookedEvents.ContainsKey(__instance))
        {
            if (HookedEvents[__instance] != null && HookedEvents[__instance] is Action)
            {
                ((Action)HookedEvents[__instance]).Invoke();
            }
        }
    }
}
