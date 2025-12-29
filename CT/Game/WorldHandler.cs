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
        if (!isLast) return;

        var currentState = __instance.m_game.CurrentState;

        if (currentState is State.Game)
        {
            if (__instance.GameOwner == GameOwnerEnum.Server)
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
                                ObjectData objectAtMousePosition = __instance.GetObjectAtPosition(debugMouse.Box2DPosition, true, true, true, __instance.EditGroupID, new Func<ObjectData, bool>(__instance.DebugMouseFilter));

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

                                    MouseJointDef mouseJointDef = new();
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
                            if (debugMouse.Joint != null)
                            {
                                debugMouse.World.DestroyJoint(debugMouse.Joint);
                                debugMouse.Object.GameWorld = null;
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
            else if (__instance.GameOwner == GameOwnerEnum.Client)
            {
                if (ClientHandler.DebugMouse)
                {
                    if (((int)(NetTime.Now * 20) % 2) == 1)
                    {
                        var mouseBox2DPosition = __instance.GetMouseBox2DPosition();

                        var data = new SFDCTMessageData();
                        data.Type = MessageHandler.SFDCTMessageDataType.DebugMouseUpdate;
                        data.Data =
                        [
                            mouseBox2DPosition.X,
                            mouseBox2DPosition.Y,
                            Input.IsMouseLeftButtonDown,
                        ];

                        MessageHandler.Send(__instance.m_game.Client, data);
                    }
                }
            }
        }
        else if (currentState is State.GameOffline)
        {
            __instance.UpdateDebugMouse();
        }
    }

    //    if (isLast && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu)
    //    {
    //        if (__instance.GameOwner != GameOwnerEnum.Client)
    //        {
    //            UpdateDebugMouseList(__instance);
    //        }

    //        if (__instance.GameOwner != GameOwnerEnum.Server)
    //        {
    //            if (ServerMouse && ClientMouse)
    //            {
    //                m_debugMouseUpdateTime -= totalMs;
    //                if (m_debugMouseUpdateTime <= 0f)
    //                {
    //                    m_debugMouseUpdateTime = 1000f / 20f;

    //                    GameSFD gameSFD = GameSFD.Handle;
    //                    Client client = gameSFD?.Client;
    //                    if (client != null && client.IsRunning)
    //                    {
    //                        Vector2 mouseBox2DPosition = __instance.GetMouseBox2DPosition();
    //                        if (Input.IsMouseLeftButtonDown || m_debugMouseClientDeleteRequest || (!Input.IsMouseLeftButtonDown && m_debugMouseClientLastMouseLeftButton))
    //                        {
    //                            DebugMouseUpdateSignalData mouseData = new()
    //                            {
    //                                Pressed = Input.IsMouseLeftButtonDown,
    //                                Delete = m_debugMouseClientDeleteRequest,
    //                                X = mouseBox2DPosition.X,
    //                                Y = mouseBox2DPosition.Y,
    //                                ID = __instance.GameInfo.GetLocalGameUser(0).GetGameConnectionTagRemoteUniqueIdentifier(),
    //                            };

    //                            client.SendMessage(MessageType.Signal, new NetMessage.Signal.Data((NetMessage.Signal.Type)30, mouseData.Store()));
    //                            m_debugMouseClientDeleteRequest = false;
    //                        }

    //                        m_debugMouseClientLastMousePosition = mouseBox2DPosition;
    //                        m_debugMouseClientLastMouseLeftButton = Input.IsMouseLeftButtonDown;
    //                    }
    //                }

    //                if (Input.KeyDown(Keys.Delete))
    //                {
    //                    if (!m_debugMouseClientDeletePressed)
    //                    {
    //                        m_debugMouseClientDeleteRequest = true;
    //                    }

    //                    m_debugMouseClientDeletePressed = true;
    //                }
    //                else
    //                {
    //                    m_debugMouseClientDeletePressed = false;
    //                }
    //            }
    //        }
    //    }
    //}

    //private static Vector2 m_debugMouseClientLastMousePosition = Vector2.Zero;
    //private static bool m_debugMouseClientDeletePressed = false;
    //private static bool m_debugMouseClientDeleteRequest = false;
    //private static bool m_debugMouseClientLastMouseLeftButton = false;

    //private static float m_debugMouseUpdateTime = 0f;
    //private static Dictionary<long, DebugMouse> m_debugMouseList = [];

    //public static void UpdateUserDebugMouse(GameConnectionTag connectionTag, Vector2 box2DPosition, bool pressed, bool delete)
    //{
    //    if (!connectionTag.IsModerator && !connectionTag.IsHost) return;
    //    if (ServerMouseNoModerators && !connectionTag.IsHost) return;
    //    if (connectionTag.RemoteUniqueIdentifier == 0) return;

    //    long remoteUniqueIdentifier = connectionTag.RemoteUniqueIdentifier;

    //    if (!m_debugMouseList.ContainsKey(remoteUniqueIdentifier))
    //    {
    //        Logger.LogDebug($"DEBUG MOUSE: Adding {remoteUniqueIdentifier}");

    //        m_debugMouseList.Add(remoteUniqueIdentifier, new DebugMouse());
    //    }

    //    DebugMouse debugMouse = m_debugMouseList[remoteUniqueIdentifier];

    //    debugMouse.RemoteUniqueIdentifier = remoteUniqueIdentifier;
    //    debugMouse.MouseBox2DPosition = box2DPosition;
    //    if (NetTime.Now - debugMouse.LastUpdateNetTime >= 0.25f)
    //    {
    //        debugMouse.MouseLastBox2DPosition = box2DPosition;
    //    }
    //    debugMouse.MouseIsPressed = pressed;
    //    debugMouse.MouseDeleteRequest = delete;
    //    debugMouse.LastUpdateNetTime = NetTime.Now;

    //    m_debugMouseList[remoteUniqueIdentifier] = debugMouse;
    //}

    //private static void UpdateDebugMouseList(GameWorld world)
    //{
    //    if (world == null) return;

    //    foreach (long key in m_debugMouseList.Keys.ToList())
    //    {
    //        DebugMouse debugMouse = m_debugMouseList[key];

    //        if (NetTime.Now - debugMouse.LastUpdateNetTime >= 3)
    //        {
    //            Logger.LogDebug($"DEBUG MOUSE: Disposing {debugMouse.RemoteUniqueIdentifier}");

    //            debugMouse.Dispose();
    //            m_debugMouseList.Remove(key);
    //            continue;
    //        }

    //        debugMouse.Update(world);
    //        m_debugMouseList[key] = debugMouse;
    //    }
    //}
}