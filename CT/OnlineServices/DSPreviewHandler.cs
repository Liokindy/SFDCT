using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.States;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class DSPreviewHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameUser), nameof(GameUser.IsDedicatedPreview), MethodType.Getter)]
    private static bool GameUser_Getter_IsDedicatedPreview_Prefix_BetterIsDSCheck(GameUser __instance, ref bool __result)
    {
        // If the host becomes a spectator it suddenly counts as the DS preview,
        // check if the account name is empty too
        __result = __instance.IsHost && __instance.JoinedAsSpectator && __instance.UserIdentifier == 1;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
    private static void GameSFD_StateKeyDownEvent_Postfix_DSKeyInput(Keys key)
    {
        if (GameSFD.Handle.GetRunningState() is not StateDSHome) { return; }

        KeyEvent(true, key);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
    private static void GameSFD_StateKeyUpEvent_Postfix_DSKeyInput(Keys key)
    {
        if (GameSFD.Handle.GetRunningState() is not StateDSHome) { return; }

        KeyEvent(false, key);
    }

    // Pass KeyEvents to the DSHome State's client.
    private static void KeyEvent(bool down, Keys key)
    {
        Client client = (GameSFD.Handle.GetRunningState() as StateDSHome).m_game?.Client;
        if (client != null)
        {
            if (down)
            {
                client.KeyDownEvent(key);
            }
            else
            {
                client.KeyUpEvent(key);
            }
        }
    }
}
