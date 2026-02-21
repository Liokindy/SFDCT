using Box2D.XNA;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFD.States;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Sync;
using SFDCT.Sync.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class WorldHandler
{
    /// <summary>
    ///     For unknown reasons players tempt to crash when joining a game.
    ///     This is caused because a collection is being modified during its iteration.
    ///     Therefore we iterate the collection backwards so it can be modified without throwing an exception.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.FinalizeProperties))]
    private static bool GameWorld_FinalizeProperties_Prefix_Cleanup(GameWorld __instance)
    {
        __instance.b2_settings.timeStep = 0f;
        __instance.Step(__instance.b2_settings);

        for (int i = __instance.DynamicObjects.Count - 1; i >= 0; i--)
        {
            __instance.DynamicObjects.ElementAt(i).Value.FinalizeProperties();
        }

        for (int i = __instance.StaticObjects.Count - 1; i >= 0; i--)
        {
            __instance.StaticObjects.ElementAt(i).Value.FinalizeProperties();
        }

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static IEnumerable<CodeInstruction> GameWorld_Update_Transpiler_Saturation(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(783).operand = SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold);
        instructions.ElementAt(787).operand = SFDCTConfig.Get<float>(CTSettingKey.LowHealthThreshold);
        instructions.ElementAt(793).operand = SFDCTConfig.Get<float>(CTSettingKey.LowHealthSaturationFactor);

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void GameWorld_Update_PostFix_Update(GameWorld __instance, float chunkMs, float totalMs, bool isLast, bool isFirst)
    {
        UpdateDebugMouse(__instance, chunkMs, totalMs, isLast, isFirst);
    }

    internal static void UpdateDebugMouse(GameWorld world, float chunkMs, float totalMs, bool isLast, bool isFirst)
    {
        if (!isLast) return;

        var currentState = world.m_game.CurrentState;

        if (currentState is State.GameOffline)
        {
            world.UpdateDebugMouse();
            return;
        }

        if (currentState is not State.Game) return;

        if (world.GameOwner == GameOwnerEnum.Server)
        {
            if (ServerHandler.DebugMouse)
            {
                var inactiveDebugMouseList = new List<DebugMouse>();

                foreach (var debugMouse in ServerHandler.DebugMouseList)
                {
                    if (debugMouse.Pressed)
                    {
                        debugMouse.LastNetUpdateTime = (float)NetTime.Now;

                        if (debugMouse.Object == null)
                        {
                            ObjectData objectAtMousePosition = world.GetObjectAtPosition(debugMouse.Box2DPosition, true, true, true, world.EditGroupID, new Func<ObjectData, bool>(world.DebugMouseFilter));

                            if (objectAtMousePosition != null)
                            {
                                objectAtMousePosition.Body.SetAwake(true);

                                float mass = objectAtMousePosition.Body.GetMass();
                                List<Body> connectedWeldedBodies = objectAtMousePosition.Body.GetConnectedWeldedBodies();
                                if (connectedWeldedBodies != null)
                                {
                                    foreach (Body connectedWeldedBody in connectedWeldedBodies)
                                    {
                                        mass += connectedWeldedBody.GetMass();
                                    }
                                }

                                var mouseJointDef = new MouseJointDef();
                                mouseJointDef.target = debugMouse.Box2DPosition;
                                mouseJointDef.localAnchor = objectAtMousePosition.Body.GetLocalPoint(mouseJointDef.target);
                                mouseJointDef.maxForce = mass * 150;
                                mouseJointDef.dampingRatio = 1;
                                mouseJointDef.frequencyHz = 40;
                                mouseJointDef.collideConnected = false;
                                mouseJointDef.bodyA = objectAtMousePosition.Body.GetWorld().GroundBody;
                                mouseJointDef.bodyB = objectAtMousePosition.Body;

                                debugMouse.Object = objectAtMousePosition;
                                debugMouse.World = objectAtMousePosition.Body.GetWorld();
                                debugMouse.Joint = (MouseJoint)objectAtMousePosition.Body.GetWorld().CreateJoint(mouseJointDef);
                            }
                        }
                        else if (debugMouse.Object != null && !debugMouse.Object.IsDisposed)
                        {
                            debugMouse.Joint.SetTarget(debugMouse.Box2DPosition);

                            if (debugMouse.Object.IsPlayer)
                            {
                                Player player = (Player)debugMouse.Object.InternalData;

                                if (!player.IsRemoved && !player.Falling)
                                {
                                    player.Fall();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (debugMouse.Object != null) debugMouse.Object = null;
                        if (debugMouse.Joint != null && debugMouse.World != null)
                        {
                            debugMouse.World.DestroyJoint(debugMouse.Joint);
                            debugMouse.Joint = null;
                            debugMouse.World = null;
                        }
                    }

                    if (NetTime.Now - debugMouse.LastNetUpdateTime >= 3)
                    {
                        inactiveDebugMouseList.Add(debugMouse);
                    }
                }

                foreach (var inactiveDebugMouse in inactiveDebugMouseList)
                {
                    ServerHandler.DebugMouseList.Remove(inactiveDebugMouse);
                }
            }
        }
        else if (world.GameOwner == GameOwnerEnum.Client)
        {
            if (ClientHandler.DebugMouse)
            {
                if (((int)(NetTime.Now * 20) % 2) == 1)
                {
                    var mouseBox2DPosition = world.GetMouseBox2DPosition();

                    var data = new SFDCTMessageData();
                    data.Type = MessageHandler.SFDCTMessageDataType.DebugMouseUpdate;
                    data.Data =
                    [
                        mouseBox2DPosition.X,
                        mouseBox2DPosition.Y,
                        Input.IsMouseLeftButtonDown,
                    ];

                    MessageHandler.Send(world.m_game.Client, data);
                }
            }
        }
    }
}