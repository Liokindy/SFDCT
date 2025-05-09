using System.Linq;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.States;
using HarmonyLib;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class WorldHandler
{
    public static bool EditorDebug = false;
    private static bool m_deletePressed = false;

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void Update(float chunkMs, float totalMs, bool isLast, bool isFirst, GameWorld __instance)
    {
        if (isLast && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu && __instance.GameOwner != GameOwnerEnum.Client)
        {
            if (EditorDebug)
            {
                __instance.UpdateDebugMouse();

                if (m_deletePressed && !Helper.Keyboard.KeyDown(Keys.Delete))
                {
                    m_deletePressed = false;
                }

                if (Helper.Keyboard.KeyDown(Keys.Delete) && !m_deletePressed)
                {
                    m_deletePressed = true;
                    __instance.DeleteObjectAtCursor();
                }
            }
        }
    }
}