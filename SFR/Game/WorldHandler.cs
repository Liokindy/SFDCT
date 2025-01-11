using System;
using System.Linq;
using SFD;
using SFD.Sounds;
using SFDCT.Helper;
using CSettings = SFDCT.Settings.Values;
using HarmonyLib;

namespace SFDCT.Game;

/// <summary>
///     This class contain patches that affect all the rounds, such as how the game is supposed to dispose objects.
/// </summary>
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

    /// <summary>
    ///     This class will be called at the end of every round.
    ///     Use it to dispose your collections or reset some data.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.DisposeAllObjects))]
    private static void DisposeData()
    {
        // SyncHandler.Attempts.Clear();
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void SaturationPatch(GameWorld __instance, float chunkMs, float totalMs, bool isLast, bool isFirst)
    {
        if (!SFD.Program.IsGame || !(__instance.EditMode & !__instance.EditPhysicsRunning) && __instance.GameOwner != GameOwnerEnum.Server)
        {
            float highestPlayerHealthFullness = 0f;
            for (int i = 0; i < __instance.LocalPlayers.Length; i++)
            {
                Player localPlayer = __instance.LocalPlayers[i];
                if (localPlayer != null && !localPlayer.IsDisposed && !localPlayer.IsDead)
                {
                    highestPlayerHealthFullness = Math.Max(highestPlayerHealthFullness, localPlayer.Health.Fullness);
                }
            }

            if (highestPlayerHealthFullness > 0f)
            {
                if (GameSFD.GUIMode == ShowGUIMode.HideAll)
                {
                    GameSFD.Saturation = 1f;
                }
                else if (highestPlayerHealthFullness < CSettings.GetFloat("LOW_HEALTH_THRESHOLD"))
                {
                    float lowhpFactor = 1f - highestPlayerHealthFullness /  CSettings.GetFloat("LOW_HEALTH_THRESHOLD");

                    if (__instance.m_nextHeartbeatDelay < 400f && highestPlayerHealthFullness < 0.25)
                    {
                        __instance.m_nextHeartbeatDelay += totalMs * Math.Max(1 - highestPlayerHealthFullness / 0.25f, 0.6f);
                    }

                    __instance.m_nextHeartbeatDelay -= totalMs * Math.Max(lowhpFactor, 0.6f);
                    if (__instance.m_nextHeartbeatDelay <= 0f)
                    {
                        Logger.LogDebug(__instance.m_nextHeartbeatDelay);
                        __instance.m_nextHeartbeatDelay = 400f;
                        SoundHandler.PlaySound("Heartbeat", 1f, __instance);
                    }

                    GameSFD.Saturation = 1f - lowhpFactor * CSettings.GetFloat("LOW_HEALTH_SATURATION_FACTOR");
                }
            }
        }
    }
    
    /*
    // Modify valid map commands
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.RunGameCommand))]
    private static bool GameWorldRunGameCommand(GameWorld __instance, string command, bool startup = false, bool forceRunCommand = false)
    {
        if (__instance.GameOwner == GameOwnerEnum.Client) { throw new Exception("Error: GameWorld.RunGameCommand() is SERVER/LOCAL ONLY"); }
        if (string.IsNullOrEmpty(command) || !command.StartsWith("/")) { return false; }

        string[] availableMapCommands =
        [
            "/SETSTARTHEALTH",
            "/SETSTARTLIFE",
            "/STARTHEALTH",
            "/STARTLIFE",
            "/MSG",
            "/MESSAGE",
            "/STARTITEMS",
            "/STARTITEM",
            "/SETSTARTUPITEM",
            "/SETSTARTUPITEMS",
            "/SETSTARTITEM",
            "/SETSTARTITEMS",
            "/INFINITE_ENERGY",
            "/IE",
            "/INFINITE_AMMO",
            "/IA",
            "/INFINITE_LIFE",
            "/IL",
            "/INFINITE_HEALTH",
            "/IH",
            "/SETTIME",
            "/REMOVE",
            "/GIVE",
            //
        ];

        bool runCommand = forceRunCommand;
        if (!runCommand)
        {
            if (__instance.CurrentActiveScriptIsExtension)
            {
                runCommand = (command.ToLowerInvariant() != "/r" && !command.ToLowerInvariant().StartsWith("/r "));
            }
            else
            {
                string text = command.ToUpperInvariant();
                foreach (string mapCommand in availableMapCommands)
                {
                    runCommand = text.StartsWith(mapCommand);
                    if (runCommand) { break; }
                }
            }
        }

        if (runCommand)
        {
            HandleCommandArgs handleCommandArgs = new HandleCommandArgs()
            {
                Command = command,
                UserIdentifier = 1,
                Origin = startup ? HandleCommandOrigin.Startup : HandleCommandOrigin.Server,
            };

            if (!__instance.GameInfo.HandleCommand(handleCommandArgs, false) && SFD.Program.IsGame && __instance.m_game.CurrentState == SFD.States.State.EditorTestRun)
            {
                MessageStack.Show(string.Format("Command '{0}' failed or not valid.", command), MessageStackType.Warning);
            }
        }
        else if (SFD.Program.IsGame && __instance.m_game.CurrentState == SFD.States.State.EditorTestRun)
        {
            MessageStack.Show(string.Format("Command '{0}' is not a valid game map command", command), MessageStackType.Error);
        }

        return false;
    }
    */
}