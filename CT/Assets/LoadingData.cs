namespace SFDCT.Assets;

internal struct LoadingData
{
    internal int Total;
    internal int Current;

    public override readonly string ToString()
    {
        return $"{Current}/{Total}";
    }
}
