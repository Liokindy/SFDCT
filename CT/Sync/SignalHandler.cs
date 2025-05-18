using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFD.Core;
using System;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class SignalHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.Signal), nameof(NetMessage.Signal.Write))]
    private static bool Write(ref NetOutgoingMessage __result, ref NetMessage.Signal.Data messageToWrite, NetOutgoingMessage netOutgoingMessage)
    {
        NetMessage.WriteDataType(MessageType.Signal, netOutgoingMessage);
        netOutgoingMessage.WriteRangedInteger(0, 31, (int)messageToWrite.Signal);

        switch (messageToWrite.Signal)
        {
            case (NetMessage.Signal.Type)30:
                object[] customSignalData = (object[])messageToWrite.Object;
                int customSignalType = (int)customSignalData[0];

                // Logger.LogDebug("WRITE START CUSTOM SIGNAL: " + customSignalType);

                netOutgoingMessage.Write(customSignalType);

                switch ((CustomSignalType)customSignalType)
                {
                    case CustomSignalType.DebugMouseUpdateSignal:
                        netOutgoingMessage = DebugMouseUpdateSignalData.Write(netOutgoingMessage, DebugMouseUpdateSignalData.Get(customSignalData));
                        break;
                    case CustomSignalType.EditorDebugFlagSignal:
                        netOutgoingMessage = EditorDebugFlagSignalData.Write(netOutgoingMessage, EditorDebugFlagSignalData.Get(customSignalData));
                        break;
                }

                // Logger.LogDebug("WRITE END CUSTOM SIGNAL: " + customSignalType);
                break;
            case NetMessage.Signal.Type.LoadSignalSpectatorMode:
            case NetMessage.Signal.Type.LoadSignal:
                netOutgoingMessage.Write(((Guid)messageToWrite.Object).ToByteArray());
                break;
            case NetMessage.Signal.Type.LoadBeginFetchingDataSignal:
                netOutgoingMessage.Write((int)messageToWrite.Object);
                break;
            case NetMessage.Signal.Type.LoadRequestDataPacketNrSignal:
                Pair<int, int> pair = (Pair<int, int>)messageToWrite.Object;
                netOutgoingMessage.Write(pair.ItemA);
                netOutgoingMessage.Write(pair.ItemB);
                break;
            case NetMessage.Signal.Type.GameOverUpdateSignal:
            case NetMessage.Signal.Type.GameOverSignal:
                GameWorld.GameOverResultUpdate gameOverResultUpdate = (GameWorld.GameOverResultUpdate)messageToWrite.Object;
                netOutgoingMessage.WriteRangedInteger(0, 20, gameOverResultUpdate.GameOverResult.GameOverMaxVotes);
                netOutgoingMessage.WriteRangedInteger(0, 20, gameOverResultUpdate.GameOverResult.GameOverVotes);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.GameOverTimeLeft);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.IsOver);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.GameOverGibOnTimesUp);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.GameOverContinueVotesDone);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.GameOverTimeDone);
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.GameOverScoreUpdated);
                netOutgoingMessage.WriteRangedInteger(0, 7, (int)gameOverResultUpdate.GameOverResult.Reason);
                netOutgoingMessage.WriteRangedInteger(0, 8, (int)gameOverResultUpdate.GameOverResult.Team);
                netOutgoingMessage.WriteRangedInteger(0, 7, (int)gameOverResultUpdate.GameOverResult.GameOverType);
                netOutgoingMessage.WriteRangedInteger(0, 31, gameOverResultUpdate.GameOverResult.WinningUserIdentifiers.Count);
                foreach (int num in gameOverResultUpdate.GameOverResult.WinningUserIdentifiers)
                {
                    netOutgoingMessage.Write(num);
                }
                netOutgoingMessage.Write(gameOverResultUpdate.GameOverResult.Text);
                break;
        }

        __result = netOutgoingMessage;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.Signal), nameof(NetMessage.Signal.Read))]
    private static bool Read(ref NetMessage.Signal.Data __result, NetIncomingMessage netIncomingMessage)
    {
        NetMessage.Signal.Data data = new NetMessage.Signal.Data();
        data.Signal = (NetMessage.Signal.Type)netIncomingMessage.ReadRangedInteger(0, 31);

        switch (data.Signal)
        {
            case (NetMessage.Signal.Type)30:
                int customSignalType = netIncomingMessage.ReadInt32();

                // Logger.LogDebug("READ START CUSTOM SIGNAL: " + customSignalType);

                switch ((CustomSignalType)customSignalType)
                {
                    case CustomSignalType.DebugMouseUpdateSignal:
                        DebugMouseUpdateSignalData customSignalData0 = new();
                        netIncomingMessage = DebugMouseUpdateSignalData.Read(netIncomingMessage, ref customSignalData0);

                        data.Object = customSignalData0.Store();
                        break;
                    case CustomSignalType.EditorDebugFlagSignal:
                        EditorDebugFlagSignalData customSignalData1 = new();
                        netIncomingMessage = EditorDebugFlagSignalData.Read(netIncomingMessage, ref customSignalData1);

                        data.Object = customSignalData1.Store();
                        break;
                }

                // Logger.LogDebug("READ END CUSTOM SIGNAL: " + customSignalType);
                break;
            case NetMessage.Signal.Type.LoadSignalSpectatorMode:
            case NetMessage.Signal.Type.LoadSignal:
                data.Object = new Guid(netIncomingMessage.ReadBytes(16));
                break;
            case NetMessage.Signal.Type.GameOverSignal:
            case NetMessage.Signal.Type.GameOverUpdateSignal:
                GameWorld.GameOverResultUpdate gameOverResultUpdate = new GameWorld.GameOverResultUpdate(new GameWorld.GameOverResultData(GameOwnerEnum.Server));
                gameOverResultUpdate.GameOverResult.GameOverMaxVotes = netIncomingMessage.ReadRangedInteger(0, 20);
                gameOverResultUpdate.GameOverResult.GameOverVotes = netIncomingMessage.ReadRangedInteger(0, 20);
                gameOverResultUpdate.GameOverResult.GameOverTimeLeft = netIncomingMessage.ReadInt32();
                gameOverResultUpdate.GameOverResult.IsOver = netIncomingMessage.ReadBoolean();
                gameOverResultUpdate.GameOverResult.GameOverGibOnTimesUp = netIncomingMessage.ReadBoolean();
                gameOverResultUpdate.GameOverResult.GameOverContinueVotesDone = netIncomingMessage.ReadBoolean();
                gameOverResultUpdate.GameOverResult.GameOverTimeDone = netIncomingMessage.ReadBoolean();
                gameOverResultUpdate.GameOverResult.GameOverScoreUpdated = netIncomingMessage.ReadBoolean();
                gameOverResultUpdate.GameOverResult.Reason = (GameWorld.GameOverReason)netIncomingMessage.ReadRangedInteger(0, 7);
                gameOverResultUpdate.GameOverResult.Team = (Team)netIncomingMessage.ReadRangedInteger(0, 8);
                gameOverResultUpdate.GameOverResult.GameOverType = (GameWorld.GameOverType)netIncomingMessage.ReadRangedInteger(0, 7);
                int num = netIncomingMessage.ReadRangedInteger(0, 31);
                for (int i = 0; i < num; i++)
                {
                    gameOverResultUpdate.GameOverResult.WinningUserIdentifiers.Add(netIncomingMessage.ReadInt32());
                }
                gameOverResultUpdate.GameOverResult.Text = netIncomingMessage.ReadString();
                data.Object = gameOverResultUpdate;
                break;
            case NetMessage.Signal.Type.LoadBeginFetchingDataSignal:
                data.Object = netIncomingMessage.ReadInt32();
                break;
            case NetMessage.Signal.Type.LoadRequestDataPacketNrSignal:
                int key = netIncomingMessage.ReadInt32();
                int value = netIncomingMessage.ReadInt32();
                data.Object = new Pair<int, int>(key, value);
                break;
        }

        __result = data;
        return false;
    }
}
