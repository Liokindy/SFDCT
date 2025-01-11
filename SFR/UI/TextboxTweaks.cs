using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFD.MenuControls;
using SFDCT.Helper;
using Microsoft.Xna.Framework.Input;
using HarmonyLib;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class TextboxTweaks
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TextBox), nameof(TextBox.KeyPress))]
    private static bool KeyPress(TextBox __instance, Keys key)
    {
        if (Helper.Keyboard.IsLeftCtrlDown && __instance.Text.Length > 0 && key == Keys.Back)
        {
            string textboxText = __instance.Text;
            int charCount = 0;

            for (int i = textboxText.Length - 1; i >= 0; i--)
            {
                char c = textboxText[i];
                if ((char.IsSeparator(c) || char.IsSymbol(c)) && i != textboxText.Length - 1)
                {
                    if (i - 1 >= 0)
                    {
                        char nextC = textboxText[i - 1];
                        if (char.IsSeparator(nextC) || char.IsSymbol(nextC))
                        {
                            charCount++;
                            continue;
                        }
                    }
                    break;
                }
                charCount++;
            }

            if (charCount != 0)
            {
                textboxText = textboxText.Substring(0, textboxText.Length - charCount);
                __instance.SetText(textboxText);
            }
            return false;
        }
        return true;
    }
}
