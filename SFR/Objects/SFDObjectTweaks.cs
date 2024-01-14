using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;
using Microsoft.Xna.Framework;

namespace SFDCT.Objects;

/// <summary>
///     Tweaks to vanilla SFD.Objects classes.
/// </summary>
[HarmonyPatch]
internal static class SFDObjectTweaks
{
    // Patch most objects using PlaySound
    // without including a position argument

    internal static readonly object method_ObjectDataGetWorldPosition = AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition));
    //private static readonly string SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound = nameof(SoundHandler.PlaySound);
    //private static readonly System.Type[] SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld = new System.Type[] 
    //{
    //    typeof(string),
    //    typeof(Microsoft.Xna.Framework.Vector2),
    //    typeof(GameWorld)
    //};

    // ObjectData, this fixes most objects getting destroyed.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.OnDestroyGenericCheck))]
    private static IEnumerable<CodeInstruction> OnDestroyGenericCheck(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(29, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(30, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
        return code;
    }

    // Objects that play sounds on their own
    [HarmonyPatch]
    private static class SFDObjectBarrelExplosive
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectBarrelExplosive), nameof(ObjectBarrelExplosive.OnDestroyObject))]
        private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            // Load the object (this.) to the stack
            code.Insert(86, new CodeInstruction(OpCodes.Ldarg_0));

            // Call this.GetWorldPosition()
            code.Insert(87, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));

            // Change PlaySound method used to the one that uses a position,
            // we change element 90 since we added 2 elements, the original
            // instruction is at 88.
            code.ElementAt(90).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
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
            code.ElementAt(52).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectC4Thrown
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.Initialize))]
        private static IEnumerable<CodeInstruction> Initialize(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(47, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(48, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(51).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.PropertyValueChanged))]
        private static IEnumerable<CodeInstruction> PropertyValueChanged(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(32, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(35).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.UpdateObject))]
        private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(56, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(57, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(60).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectGibZone
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectGibZone), nameof(ObjectGibZone.UpdateObjectBeforeBox2DStep))]
        private static IEnumerable<CodeInstruction> UpdateObjectBeforeBox2DStep(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            // Load the soon-to-be removed player to the stack, instead of the ObjectGibZone
            code.Insert(86, new CodeInstruction(OpCodes.Ldloc_3));
            code.Insert(87, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(90).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectGrenadeThrown
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectGrenadeThrown), nameof(ObjectGrenadeThrown.UpdateObject))]
        private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(37, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(38, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(41).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectHelicopter
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectHelicopter), nameof(ObjectHelicopter.OnDestroyObject))]
        private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(33, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(34, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(37).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectMineThrown
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectMineThrown), nameof(ObjectMineThrown.UpdateObject))]
        private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(34, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(35, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(38).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectPlant
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectPlant), nameof(ObjectPlant.OnDestroyObject))]
        private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(23, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(24, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(27).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
    }

#region STREETSWEEPER
    [HarmonyPatch]
    private static class SFDObjectStreetsweeper
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.TakeImpactDamage))]
        private static IEnumerable<CodeInstruction> TakeImpactDamage(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(10, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(11, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(14).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.TakeProjectileDamage))]
        private static IEnumerable<CodeInstruction> TakeProjectileDamage(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.Insert(8, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(9, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            code.ElementAt(12).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.UpdateBlinking))]
        private static IEnumerable<CodeInstruction> UpdateBlinking(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            // The original code specifies the volume to be 1f,
            // we'll replace those lines instead of adding 2 new ones
            // since sounds already play at 1f volume by default.
            code.ElementAt(26).opcode = OpCodes.Ldarg_0;
            code.ElementAt(26).operand = null;

            code.Insert(27, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));

            // Offset is 1 instead of 2. 29+1 = 30
            code.ElementAt(30).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            return code;
        }
        
    }

    [HarmonyPatch]
    private static class SFDObjectStreetsweeperCrate
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeperCrate), nameof(ObjectStreetsweeperCrate.OnActivated))]
        private static IEnumerable<CodeInstruction> OnActivated(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.ElementAt(18).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(22).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            code.Insert(16, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(17, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));

            code.Insert(22, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(23, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeperCrate), nameof(ObjectStreetsweeperCrate.Open))]
        private static IEnumerable<CodeInstruction> Open(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            // We do this first so we dont take offsets into account
            code.ElementAt(54).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.ElementAt(58).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);

            // First, no offset
            code.Insert(52, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(53, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));

            // Second, 2 offset
            code.Insert(58, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(59, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            return code;
        }
    }

    [HarmonyPatch]
    private static class SFDObjectStreetsweeperWreck
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectStreetsweeperWreck), nameof(ObjectStreetsweeperWreck.UpdateObject))]
        private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.ElementAt(49).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(47, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(48, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            return code;
        }
    }
    #endregion

    [HarmonyPatch]
    private static class SFDObjectWoodSupport
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObjectWoodSupport), nameof(ObjectWoodSupport.OnDestroyObject))]
        private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            code.ElementAt(31).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundHandler.nameof_soundHandlerPlaySound, SFDCT.Game.SoundHandler.typeof_StringVector2Gameworld);
            code.Insert(29, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(30, new CodeInstruction(OpCodes.Call, method_ObjectDataGetWorldPosition));
            return code;
        }
    }
}