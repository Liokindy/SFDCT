using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFR.Objects;


[HarmonyPatch]
internal static class SFDObjectTweaks
{
    private static readonly object method_ObjectDataGetWorldPosition = AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition));
    private static readonly string nameof_soundHandlerPlaySound = nameof(SoundHandler.PlaySound);
    private static readonly System.Type[] typeof_StringVector2Gameworld = new System.Type[] 
    {
        typeof(string),
        typeof(Microsoft.Xna.Framework.Vector2),
        typeof(GameWorld)
    };

    [HarmonyPatch]
    private static class SFDObjectBarrelExplosive
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectBarrelExplosive), nameof(ObjectBarrelExplosive.OnDestroyObject))]
        private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(86, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(87, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(90).operand = AccessTools.Method(typeof(SoundHandler), nameof_soundHandlerPlaySound, typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectBird
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectBird), nameof(ObjectBird.UpdateObject))]
        private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(48, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(49, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(50).operand = AccessTools.Method(typeof(SoundHandler), nameof_soundHandlerPlaySound, typeof_StringVector2Gameworld);
            return code;
        }
    }



}