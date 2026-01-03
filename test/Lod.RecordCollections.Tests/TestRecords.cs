namespace Lod.RecordCollections.Tests;

internal sealed record Number
{
    public int Value { get; set; }

    public Number() { }

    public Number(int value)
    {
        Value = value;
    }
}
