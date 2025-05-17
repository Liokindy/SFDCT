using Lidgren.Network;
using SFD;

namespace SFDCT.Sync;

internal class EditorDebugFlagSignalData
{
    public bool Enabled;

    public object[] Store()
    {
        return [
            (int)CustomSignalType.EditorDebugFlagSignal,
            this.Enabled,
        ];
    }

    public static EditorDebugFlagSignalData Get(object[] data)
    {
        return new()
        {
            Enabled = (bool)data[1],
        };
    }

    public static NetOutgoingMessage Write(NetOutgoingMessage netOutgoingMessage, EditorDebugFlagSignalData signalData)
    {
        netOutgoingMessage.Write(signalData.Enabled);

        return netOutgoingMessage;
    }

    public static NetIncomingMessage Read(NetIncomingMessage netIncomingMessage, ref EditorDebugFlagSignalData signalData)
    {
        signalData.Enabled = netIncomingMessage.ReadBoolean();

        return netIncomingMessage;
    }
}

internal class DebugMouseUpdateSignalData
{
    public bool Pressed;
    public bool Delete;
    public float X;
    public float Y;
    public long ID;

    public object[] Store()
    {
        return [
            (int)CustomSignalType.DebugMouseUpdateSignal,
            this.Pressed,
            this.Delete,
            this.X,
            this.Y,
            this.ID,
        ];
    }

    public static DebugMouseUpdateSignalData Get(object[] data)
    {
        return new()
        {
            Pressed = (bool)data[1],
            Delete = (bool)data[2],
            X = (float)data[3],
            Y = (float)data[4],
            ID = (long)data[5],
        };
    }

    public static NetOutgoingMessage Write(NetOutgoingMessage netOutgoingMessage, DebugMouseUpdateSignalData signalData)
    {
        netOutgoingMessage.Write(signalData.Pressed);
        netOutgoingMessage.Write(signalData.Delete);
        netOutgoingMessage.Write(signalData.X);
        netOutgoingMessage.Write(signalData.Y);

        return netOutgoingMessage;
    }

    public static NetIncomingMessage Read(NetIncomingMessage netIncomingMessage, ref DebugMouseUpdateSignalData signalData)
    {
        signalData.Pressed = netIncomingMessage.ReadBoolean();
        signalData.Delete = netIncomingMessage.ReadBoolean();
        signalData.X = netIncomingMessage.ReadSingle();
        signalData.Y = netIncomingMessage.ReadSingle();

        signalData.ID = netIncomingMessage.GameConnectionTag() != null ? netIncomingMessage.GameConnectionTag().RemoteUniqueIdentifier : 0;

        return netIncomingMessage;
    }
}
