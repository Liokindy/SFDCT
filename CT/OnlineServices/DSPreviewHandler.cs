using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.States;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class DSPreviewHandler
{
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
