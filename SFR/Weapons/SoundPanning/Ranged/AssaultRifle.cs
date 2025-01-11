using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Weapons;

namespace SFDCT.Weapons.SoundPanning.Ranged
{
    [HarmonyPatch]
    internal static class AssaultRifle
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WpnAssaultRifle), nameof(WpnAssaultRifle.OnSubAnimationEvent))]
        private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            // Change PlaySound method so it uses a position,
            // do this before inserting so we dont offset the index.
            code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
            code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

            // Load the player var to the stack
            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));

            // Get player.Position
            code.Insert(17, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

            // Next PlaySound
            code.Insert(25, new CodeInstruction(OpCodes.Ldarg_1)); // Load player into stack
            code.Insert(26, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position)))); // Get player.Position

            return code;
        }
    }
}
