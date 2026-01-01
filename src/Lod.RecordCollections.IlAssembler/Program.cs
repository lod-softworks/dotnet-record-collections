using System.Diagnostics;

Console.WriteLine($"Preparing IL modification of Lod.RecordCollections to '{args.FirstOrDefault()}'");

string dllPath = !string.IsNullOrWhiteSpace(args.FirstOrDefault()) ? args.First()
    : throw new ArgumentException("Lod.RecordCollections DLL not found in args.");
if (!File.Exists(dllPath)) throw new FileNotFoundException("Lod.RecordCollections.dll");

static string GetSearchDirectory()
{
    // Prefer the app base directory (more deterministic/safer than CWD), fallback to the current directory.
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    return !string.IsNullOrWhiteSpace(baseDir) ? baseDir : Directory.GetCurrentDirectory();
}

static string FindToolPath(string searchDirectory, string fileName)
{
    string searchRoot = Path.GetFullPath(searchDirectory);

    string? candidate = Directory.EnumerateFiles(searchRoot, fileName, SearchOption.AllDirectories).FirstOrDefault();
    if (candidate == null) throw new FileNotFoundException(fileName);

    string fullCandidate = Path.GetFullPath(candidate);
    if (!string.Equals(Path.GetFileName(fullCandidate), fileName, StringComparison.OrdinalIgnoreCase))
    {
        throw new FileNotFoundException($"Resolved tool does not match expected filename '{fileName}'.", fullCandidate);
    }

    // Ensure the discovered tool is under our intended search root.
    if (!fullCandidate.StartsWith(searchRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
    {
        throw new FileNotFoundException($"Resolved tool '{fileName}' is outside of the search directory.", fullCandidate);
    }

    return fullCandidate;
}

static async Task<(int ExitCode, string StdOut, string StdErr)> RunProcessAsync(string fileName, string arguments, string workingDirectory)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    };

    using Process process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Failed to start process '{fileName}'.");

    Task<string> stdOutTask = process.StandardOutput.ReadToEndAsync();
    Task<string> stdErrTask = process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();
    string stdOut = await stdOutTask;
    string stdErr = await stdErrTask;

    return (process.ExitCode, stdOut, stdErr);
}

string directory = GetSearchDirectory();
Console.WriteLine($"Searching for ilasm/ildasm in directory '{directory}'.");
string ilasmPath = FindToolPath(directory, "ilasm.exe");
string ildasmPath = FindToolPath(directory, "ildasm.exe");


// decompile
Console.WriteLine("Decompiling IL.");

string fileName = "Lod.RecordCollections.il";
string ilPath = Path.Combine(directory, fileName);

var (dasmExitCode, dasmOut, dasmErr) = await RunProcessAsync(ildasmPath, $"\"{dllPath}\" /out:\"{ilPath}\"", directory);
if (dasmExitCode != 0)
{
    throw new InvalidOperationException($"IL decompilation failed (exit code {dasmExitCode}).\n{dasmOut}\n{dasmErr}");
}


// modify
Console.WriteLine("Modifying IL");

string fileContent = await File.ReadAllTextAsync(ilPath);
Type[] collectionNames =
[
    typeof(RecordDictionary<,>),
    typeof(RecordList<>),
    typeof(RecordQueue<>),
    typeof(RecordSet<>),
    typeof(RecordStack<>),
];
string cloneTemplate = @"
    .method public hidebysig newslot virtual 
        instance class $!TYPE!$<!T> '<Clone>$' () cil managed 
    {
        .maxstack 24
        .locals init (
            [0] class $!TYPE!$<!T>
        )

        IL_0000: nop
        IL_0001: ldarg.0
        IL_0002: newobj instance void class $!TYPE!$<!T>::.ctor(class $!TYPE!$<!0>)
        IL_0007: stloc.0
        IL_0008: br.s IL_000a

        IL_000a: ldloc.0
        IL_000b: ret
    } // end of method $!TYPE!$::'<Clone>$'
";
foreach (Type collection in collectionNames)
{
    string typeName = collection.FullName!;
    Type[] genericArgs = collection.GetGenericArguments();
    string generics = $"<{string.Join(", ", genericArgs.Select(a => $"!{a.Name}"))}>";
    string genericIndexes = $"<{string.Join(", ", genericArgs.Select((_, i) => $"!{i}"))}>";
    string template = cloneTemplate.Replace("$!TYPE!$", typeName)
        .Replace("<!T>", generics)
        .Replace("<!0>", genericIndexes);

    fileContent = fileContent.Replace($"}} // end of class {typeName}", template + "}");
}
await File.WriteAllTextAsync(ilPath, fileContent);


// recompile
Console.WriteLine("Compile IL");

string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
string ilasmArgs = $"/dll \"{ilPath}\" /output:\"{dllPath}\"";
if (File.Exists(pdbPath))
{
    ilasmArgs += $" /pdb:\"{pdbPath}\"";
}

var (asmExitCode, asmOut, asmErr) = await RunProcessAsync(ilasmPath, ilasmArgs, directory);
if (asmExitCode != 0)
{
    throw new InvalidOperationException($"IL compilation failed (exit code {asmExitCode}).\n{asmOut}\n{asmErr}");
}


#if DEBUG
Console.ReadKey();
#endif