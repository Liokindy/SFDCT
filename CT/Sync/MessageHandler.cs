using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFDCT.Sync.Data;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class MessageHandler
{
    // MessageTypes 33, 34 and 36 are unused and not read by vanilla clients
    internal static readonly NET_DELIVERY SFDCTMessageDelivery = new(NetDeliveryMethod.ReliableOrdered, 31);
    internal const MessageType SFDCTMessageType = (MessageType)33;
    internal enum SFDCTMessageDataType : byte
    {
        DebugMouseToggle,
        DebugMouseUpdate,
        ProfileChangeRequest,
    }

    internal static void Send(ClientServerBase owner, SFDCTMessageData data)
    {
        Send(owner, data, null, null);
    }

    internal static void Send(ClientServerBase owner, SFDCTMessageData data, NetConnection except, NetConnection single)
    {
        NetOutgoingMessage outgoingMessage;

        if (owner is Client ownerClient)
        {
            outgoingMessage = Write(ownerClient.m_client.CreateMessage(), data);

            ownerClient.m_client.SendMessage(outgoingMessage, SFDCTMessageDelivery.Method, SFDCTMessageDelivery.Channel);
        }
        else if (owner is Server ownerServer)
        {
            outgoingMessage = Write(ownerServer.m_server.CreateMessage(), data);

            if (single == null)
            {
                ownerServer.m_server.SendToAll(outgoingMessage, except, SFDCTMessageDelivery.Method, SFDCTMessageDelivery.Channel);
            }
            else
            {
                ownerServer.m_server.SendMessage(outgoingMessage, single, SFDCTMessageDelivery.Method, SFDCTMessageDelivery.Channel);
            }
        }
    }

    internal static SFDCTMessageData Read(NetIncomingMessage incomingMessage, NetMessage.MessageData messageData)
    {
        // Logger.LogDebug("Reading SFDCTMessage");
        var data = new SFDCTMessageData();

        data.Type = (SFDCTMessageDataType)incomingMessage.ReadByte();
        switch (data.Type)
        {
            case SFDCTMessageDataType.DebugMouseToggle:
                data.Data =
                [
                    incomingMessage.ReadBoolean(),
                ];
                break;
            case SFDCTMessageDataType.DebugMouseUpdate:
                Vector2 position = incomingMessage.ReadVector2Position();
                data.Data =
                [
                    position.X,
                    position.Y,
                    incomingMessage.ReadBoolean(),
                ];
                break;
            case SFDCTMessageDataType.ProfileChangeRequest:
                var userIndex = incomingMessage.ReadInt32();
                var profile = NetMessage.PlayerProfileMessage.Read(incomingMessage, Profile.ValidateProfileType.CanEquip);

                data.Data = [
                    userIndex,
                    profile
                ];
                break;
        }

        return data;
    }

    internal static NetOutgoingMessage Write(NetOutgoingMessage outgoingMessage, SFDCTMessageData data)
    {
        // Logger.LogDebug("Writting SFDCTMessage");
        NetMessage.WriteDataType(SFDCTMessageType, outgoingMessage);

        outgoingMessage.Write((byte)data.Type);
        switch (data.Type)
        {
            default:
                break;
            case SFDCTMessageDataType.DebugMouseToggle:
                outgoingMessage.Write((bool)data.Data[0]);
                break;
            case SFDCTMessageDataType.DebugMouseUpdate:
                outgoingMessage.WriteVector2Position(new Vector2((float)data.Data[0], (float)data.Data[1]));
                outgoingMessage.Write((bool)data.Data[2]);
                break;
            case SFDCTMessageDataType.ProfileChangeRequest:
                outgoingMessage.Write((int)data.Data[0]);
                NetMessage.PlayerProfileMessage.Write((Profile)data.Data[1], outgoingMessage);
                break;
        }

        return outgoingMessage;
    }
}
