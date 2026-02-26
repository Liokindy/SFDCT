namespace SFDCT.Sync.Data;

internal class SFDCTMessageData
{
    internal SFDCTMessageData() { }
    internal SFDCTMessageData(SFDCTMessageDataType type, params object[] data)
    {
        Type = type;
        Data = data;
    }

    internal SFDCTMessageDataType Type;
    internal object[] Data;
}
