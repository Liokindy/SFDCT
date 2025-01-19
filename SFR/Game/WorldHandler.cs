using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using CGlobals = SFDCT.Misc.Globals;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EffectHandler), nameof(EffectHandler.CreateEffect))]
    private static bool EffectHandlerCreateEffect(string effectId, Vector2 worldPosition, GameWorld gameWorld, object[] args)
    {
        if (effectId.Equals("HIT"))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    ///     Potential bug-fix for users not being able to open maps in
    ///     vanilla-SFD, might be caused by maps being saved as v.1.3.7x 
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.WriteToStream))]
    private static IEnumerable<CodeInstruction> SFDMapEditorBuildTreeViewImageList(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction code in instructions)
        {
            if (code.operand == null)
            {
                continue;
            }
            if (code.operand.Equals("v.1.3.7x"))
            {
                code.operand = CGlobals.Version.SFD;
            }
        }
        return instructions;
    }

    /// <summary>
    ///     This class will be called at the end of every round.
    ///     Use it to dispose your collections or reset some data.
    /// </summary>
    //[HarmonyPostfix]
    //[HarmonyPatch(typeof(GameWorld), nameof(GameWorld.DisposeAllObjects))]
    //private static void DisposeData()
    //{
    //    SyncHandler.Attempts.Clear();
    //}


    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static IEnumerable<CodeInstruction> SaturationPatch(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(760).operand = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.LowHealthThreshold));
        instructions.ElementAt(764).operand = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.LowHealthThreshold));
        instructions.ElementAt(770).operand = CSettings.Get<float>(CSettings.GetKey(CSettings.SettingKey.LowHealthSaturationFactor));

        return instructions;
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