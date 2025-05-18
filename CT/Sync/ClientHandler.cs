using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFDCT.Game;
using SFDCT.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ClientHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Client), nameof(Client.Shutdown))]
    private static void ClientShutdown()
    {
        WorldHandler.ServerMouse = false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Client), nameof(Client.SendMessage), [typeof(MessageType), typeof(object), typeof(NetClient)])]
    private static IEnumerable<CodeInstruction> ClientSendMessage(IEnumerable<CodeInstruction> instructions)
    {
        //for (int i = 0; i < 8; i++)
        //{
        //    instructions.ElementAt(61 + i).opcode = OpCodes.Nop;
        //}

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Client), nameof(Client.HandleDataMessage))]
    private static IEnumerable<CodeInstruction> ClientHandleDataMessage(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new(instructions);

        //for (int i = 0; i < 8; i++)
        //{
        //    code[13 + i].opcode = OpCodes.Nop;
        //}

        code.Insert(28, new CodeInstruction(OpCodes.Ldarg_0, null));
        code.Insert(29, new CodeInstruction(OpCodes.Ldarg_2, null));
        code.Insert(30, new CodeInstruction(OpCodes.Ldloc_1, null));
        code.Insert(31, new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ClientHandler), nameof(ClientHandleCustomSignal), [typeof(Client), typeof(NetIncomingMessage), typeof(NetMessage.Signal.Data)])));
        
        return code;
    }

    public static void ClientHandleCustomSignal(Client client, NetIncomingMessage msg, NetMessage.Signal.Data msgSignalData)
    {
        switch (msgSignalData.Signal)
        {
            case (NetMessage.Signal.Type)30:
                object[] data = (object[])msgSignalData.Object;
                int customSignalType = (int)data[0];

                switch ((CustomSignalType)customSignalType)
                {
                    case CustomSignalType.EditorDebugFlagSignal:
                        EditorDebugFlagSignalData customSignalData = EditorDebugFlagSignalData.Get(data);

                        WorldHandler.ServerMouse = customSignalData.Enabled;
                        // Logger.LogDebug("CLIENT: EditorDebugFlagSignal: " + editorDebugState);
                        break;
                }
                break;
        }
    }
}
