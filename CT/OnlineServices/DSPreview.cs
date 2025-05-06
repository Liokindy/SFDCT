using System;
using SFD;
using SFD.States;
using Microsoft.Xna.Framework.Input;
using HarmonyLib;

namespace SFDCT.OnlineServices;

/// <summary>
///     Pass key events to the DS preview, allows the preview to move
///     in-game if it's currently controlling an alive player.
/// </summary>
[HarmonyPatch]
internal static class DSPreview
{
    //     Request to join as a normal user instead of spectator.
    //     
    //     Non-spectators are allowed to vote and count towards alive players,
    //     meaning they can win after being the last standing or prevent other
    //     player from winning. Disabled to allow the DS to connect while the 
    //     server is full.
    //[HarmonyTranspiler]
    //[HarmonyPatch(typeof(Client), nameof(Client.CreateConnectRequestMessage))]
    //private static IEnumerable<CodeInstruction> Client_CreateConnectRequestMessage(IEnumerable<CodeInstruction> instructions)
    //{
    //    List<CodeInstruction> code = new List<CodeInstruction>(instructions);

    //    code.ElementAt(0).opcode = OpCodes.Ldc_I4_0;

    //    return code;
    //}


    //     Hook to KeyDownEvent
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
    private static void GameSFD_StateKeyDownEvent(Keys key)
    {
        if (GameSFD.Handle.GetRunningState() is not StateDSHome) { return; }

        KeyEvent(true, key);
    }

    //     Hook to KeyUpEvent
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
    private static void GameSFD_StateKeyUpEvent(Keys key)
    {
        if (GameSFD.Handle.GetRunningState() is not StateDSHome) { return; }

        KeyEvent(false, key);
    }

    //     Pass KeyEvents to the DSHome State's client.
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
