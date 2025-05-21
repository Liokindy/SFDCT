using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.States;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Sync;
using System.Collections.Generic;
using System.Linq;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class WorldHandler
{
    public static bool ServerMouse = false;
    public static bool ServerMouseNoModerators = false;
    public static bool ClientMouse = false;

    /// <summary>
    ///     For unknown reasons players tempt to crash when joining a game.
    ///     This is caused because a collection is being modified during its iteration.
    ///     Therefore we iterate the collection backwards so it can be modified without throwing an exception.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.FinalizeProperties))]
    private static bool FinalizeProperties(GameWorld __instance)
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
    private static IEnumerable<CodeInstruction> UpdateSaturation(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(783).operand = Settings.Get<float>(SettingKey.LowHealthThreshold);
        instructions.ElementAt(787).operand = Settings.Get<float>(SettingKey.LowHealthThreshold);
        instructions.ElementAt(793).operand = Settings.Get<float>(SettingKey.LowHealthSaturationFactor);

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void Update(float chunkMs, float totalMs, bool isLast, bool isFirst, GameWorld __instance)
    {
        if (isLast && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu)
        {
            if (__instance.GameOwner != GameOwnerEnum.Client)
            {
                UpdateDebugMouseList(__instance);
            }

            if (__instance.GameOwner != GameOwnerEnum.Server)
            {
                if (ServerMouse && ClientMouse)
                {
                    m_debugMouseUpdateTime -= totalMs;
                    if (m_debugMouseUpdateTime <= 0f)
                    {
                        m_debugMouseUpdateTime = 1000f / 20f;

                        GameSFD gameSFD = GameSFD.Handle;
                        Client client = gameSFD?.Client;
                        if (client != null && client.IsRunning)
                        {
                            Vector2 mouseBox2DPosition = __instance.GetMouseBox2DPosition();
                            if (Input.IsMouseLeftButtonDown || m_debugMouseClientDeleteRequest || (!Input.IsMouseLeftButtonDown && m_debugMouseClientLastMouseLeftButton))
                            {
                                DebugMouseUpdateSignalData mouseData = new()
                                {
                                    Pressed = Input.IsMouseLeftButtonDown,
                                    Delete = m_debugMouseClientDeleteRequest,
                                    X = mouseBox2DPosition.X,
                                    Y = mouseBox2DPosition.Y,
                                    ID = __instance.GameInfo.GetLocalGameUser(0).GetGameConnectionTagRemoteUniqueIdentifier(),
                                };

                                client.SendMessage(MessageType.Signal, new NetMessage.Signal.Data((NetMessage.Signal.Type)30, mouseData.Store()));
                                m_debugMouseClientDeleteRequest = false;
                            }

                            m_debugMouseClientLastMousePosition = mouseBox2DPosition;
                            m_debugMouseClientLastMouseLeftButton = Input.IsMouseLeftButtonDown;
                        }
                    }

                    if (Input.KeyDown(Keys.Delete))
                    {
                        if (!m_debugMouseClientDeletePressed)
                        {
                            m_debugMouseClientDeleteRequest = true;
                        }

                        m_debugMouseClientDeletePressed = true;
                    }
                    else
                    {
                        m_debugMouseClientDeletePressed = false;
                    }
                }
            }
        }
    }

    private static Vector2 m_debugMouseClientLastMousePosition = Vector2.Zero;
    private static bool m_debugMouseClientDeletePressed = false;
    private static bool m_debugMouseClientDeleteRequest = false;
    private static bool m_debugMouseClientLastMouseLeftButton = false;

    private static float m_debugMouseUpdateTime = 0f;
    private static Dictionary<long, DebugMouse> m_debugMouseList = [];

    public static void UpdateUserDebugMouse(GameConnectionTag connectionTag, Vector2 box2DPosition, bool pressed, bool delete)
    {
        if (!connectionTag.IsModerator && !connectionTag.IsHost)
        {
            return;
        }

        if (ServerMouseNoModerators && !connectionTag.IsHost)
        {
            return;
        }

        long remoteUniqueIdentifier = connectionTag.RemoteUniqueIdentifier;
        if (remoteUniqueIdentifier == 0)
        {
            return;
        }

        if (!m_debugMouseList.ContainsKey(remoteUniqueIdentifier))
        {
            Logger.LogDebug($"DEBUG MOUSE: Adding {remoteUniqueIdentifier}");

            m_debugMouseList.Add(remoteUniqueIdentifier, new DebugMouse());
        }

        DebugMouse debugMouse = m_debugMouseList[remoteUniqueIdentifier];

        debugMouse.RemoteUniqueIdentifier = remoteUniqueIdentifier;
        debugMouse.MouseBox2DPosition = box2DPosition;
        if (NetTime.Now - debugMouse.LastUpdateNetTime >= 0.25f)
        {
            debugMouse.MouseLastBox2DPosition = box2DPosition;
        }
        debugMouse.MouseIsPressed = pressed;
        debugMouse.MouseDeleteRequest = delete;
        debugMouse.LastUpdateNetTime = NetTime.Now;

        m_debugMouseList[remoteUniqueIdentifier] = debugMouse;
    }

    private static void UpdateDebugMouseList(GameWorld world)
    {
        if (world == null)
        {
            return;
        }

        foreach (long key in m_debugMouseList.Keys.ToList())
        {
            DebugMouse debugMouse = m_debugMouseList[key];

            if (NetTime.Now - debugMouse.LastUpdateNetTime >= 3)
            {
                Logger.LogDebug($"DEBUG MOUSE: Disposing {debugMouse.RemoteUniqueIdentifier}");

                debugMouse.Dispose();
                m_debugMouseList.Remove(key);
                continue;
            }

            debugMouse.Update(world);
            m_debugMouseList[key] = debugMouse;
        }
    }
}