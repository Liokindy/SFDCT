using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFDCT.Weapons;

/// <summary>
///     Tweaks to vanilla SFD weapons.
/// </summary>
[HarmonyPatch]
internal static class SFDWeaponTweaks
{
    // Patch weapons using PlaySound
    // without including a position argument
    // i.e: Draw sounds

    private static readonly object get_PlayerPosition = AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position));

    // Ranged weapons
    // (Handgun, Rifle)
    #region RANGE
    [HarmonyPatch]
    private static class SFDWpnAssaultRifle
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnAssaultRifle), nameof(WpnAssaultRifle.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            // Change PlaySound method so it uses a position,
            // do this before inserting so we dont offset the index.
            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            // Load the player var to the stack
            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));

            // Get player.Position
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            // Next PlaySound
            code.Insert(25, new CodeInstruction(OpCodes.Ldarg_1)); // Load player into stack
            code.Insert(26, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition)); // Get player.Position

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBazooka
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBazooka), nameof(WpnBazooka.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23+2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24+2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBow
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBow), nameof(WpnBow.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnCarbine
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnCarbine), nameof(WpnCarbine.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(25, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(26, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnDarkShotgun
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(17).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(33).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(15, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(16, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(31 + 4, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(32 + 4, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnPostFireAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnPostFireAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(51).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(59).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(49, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(50, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(57 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(58 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnReloadAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnReloadAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(14).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(12, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(13, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnFlamethrower
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnFlamethrower), nameof(WpnFlamethrower.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnFlareGun
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnFlareGun), nameof(WpnFlareGun.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnGrenadeLauncher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnGrenadeLauncher), nameof(WpnGrenadeLauncher.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnM60
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnM60), nameof(WpnM60.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnMachinePistol
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnMachinePistol), nameof(WpnMachinePistol.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnMagnum
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnMagnum), nameof(WpnMagnum.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnMP50
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnMP50), nameof(WpnMP50.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnPistol
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPistol), nameof(WpnPistol.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnPistol45
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPistol45), nameof(WpnPistol45.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnPumpShotgun
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPumpShotgun), nameof(WpnPumpShotgun.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(17).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(33).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(15, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(16, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(31 + 4, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(32 + 4, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPumpShotgun), nameof(WpnPumpShotgun.OnReloadAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnReloadAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(14).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(12, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(13, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPumpShotgun), nameof(WpnPumpShotgun.OnPostFireAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnPostFireAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(51).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(59).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(49, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(50, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(57 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(58 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
        
    }

    [HarmonyPatch]
    private static class SFDWpnRevolver
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnRevolver), nameof(WpnRevolver.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSawedOffShotgun
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSawedOffShotgun), nameof(WpnSawedOffShotgun.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSawedOffShotgun), nameof(WpnSawedOffShotgun.OnReloadAnimationFinished))]
        private static IEnumerable<CodeInstruction> OnReloadAnimationFinished(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(7).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(5, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(6, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSilencedPistol
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSilencedPistol), nameof(WpnSilencedPistol.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSilencedUzi
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSilencedUzi), nameof(WpnSilencedUzi.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSMG
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSMG), nameof(WpnSMG.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSniperRifle
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSniperRifle), nameof(WpnSniperRifle.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSniperRifle), nameof(WpnSniperRifle.OnPostFireAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnPostFireAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(51).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(59).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(49, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(50, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(57 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(58 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnTommygun
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnTommygun), nameof(WpnTommygun.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnUzi
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnUzi), nameof(WpnUzi.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));
            code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }
    #endregion

    //  Melee weapons
    //  (Makeshift, Melee)
    #region MELEE
    [HarmonyPatch]
    private static class SFDWpnAxe
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnAxe), nameof(WpnAxe.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBaseball
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBaseball), nameof(WpnBaseball.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBat
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBat), nameof(WpnBat.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBaton
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBaton), nameof(WpnBaton.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnBrokenBottle
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnBrokenBottle), nameof(WpnBrokenBottle.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnChain
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChain), nameof(WpnChain.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnChainsaw
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(52).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            code.Insert(54, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(55, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(30).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(35).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            code.Insert(28 + 2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(29 + 2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            code.Insert(33 + 4, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(34 + 4, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.UpdateExtraMeleeState))]
        private static IEnumerable<CodeInstruction> UpdateExtraMeleeState(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(155).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(203).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(153, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(154, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            code.Insert(201+2, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(202+2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnChair
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChair), nameof(WpnChair.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnChairLeg
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnChairLeg), nameof(WpnChairLeg.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnCueStick
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnCueStick), nameof(WpnCueStick.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnCueStickShaft
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnCueStickShaft), nameof(WpnCueStickShaft.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnFlagpole
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnFlagpole), nameof(WpnFlagpole.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnHammer
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnHammer), nameof(WpnHammer.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnKatana
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnKatana), nameof(WpnKatana.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnKnife
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnKnife), nameof(WpnKnife.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnLeadPipe
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnLeadPipe), nameof(WpnLeadPipe.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnMachete
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnMachete), nameof(WpnMachete.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnPipeWrench
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnPipeWrench), nameof(WpnPipeWrench.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnShockBaton
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnShockBaton), nameof(WpnShockBaton.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnSuitcase
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnSuitcase), nameof(WpnSuitcase.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnTeapot
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnTeapot), nameof(WpnTeapot.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnTrashcanLid
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnTrashcanLid), nameof(WpnTrashcanLid.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDWpnWhip
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnWhip), nameof(WpnWhip.Destroyed))]
        private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundHandler.typeof_soundHandler, SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
            code.Insert(2, new CodeInstruction(OpCodes.Callvirt, get_PlayerPosition));

            return code;
        }
    }
    #endregion
}
