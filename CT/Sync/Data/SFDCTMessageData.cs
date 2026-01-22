namespace SFDCT.Sync.Data;

internal class SFDCTMessageData
{
    internal SFDCTMessageData() { }
    internal SFDCTMessageData(MessageHandler.SFDCTMessageDataType type, params object[] data)
    {
        Type = type;
        Data = data;
    }

    internal MessageHandler.SFDCTMessageDataType Type;
    internal object[] Data;
}
