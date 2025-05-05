//using System;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using System.Linq;
//using SFD;
//using SFD.Effects;
//using SFD.States;
//using Microsoft.Xna.Framework;
//using HarmonyLib;

//namespace SFDCT.Game.Commands;

//[HarmonyPatch]
//internal static class DebugMouse
//{
//    public static bool IsEnabled;

//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
//    private static void Update(float chunkMs, float totalMs, bool isLast, bool isFirst, GameWorld __instance)
//    {
//        if (isLast && SFD.Program.IsGame && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu && __instance.GameOwner != GameOwnerEnum.Client)
//        {
//            if (IsEnabled)
//            {
//                __instance.UpdateDebugMouse();
//            }
//        }
//    }
//}

