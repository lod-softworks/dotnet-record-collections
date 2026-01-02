[assembly: Parallelize]

namespace Lod.RecordCollections.IlModified.Tests;

public static class Program
{
    // This keeps MSTest and MSBuild happy even when the IL verification tests are excluded.
    public static void Main(string[] _) { }
}

