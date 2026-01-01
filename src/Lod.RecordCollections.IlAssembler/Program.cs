using System.Collections;
using System.Diagnostics;
using System.Reflection;

try
{
    string? inputDllPath = args.Length > 0 ? args[0] : null;
    string? outputDirectory = args.Length > 1 ? args[1] : null;

    if (string.IsNullOrWhiteSpace(inputDllPath))
        throw new ArgumentException("Lod.RecordCollections DLL not found in args.");
    if (!File.Exists(inputDllPath)) throw new FileNotFoundException("The compiled library was not found.", inputDllPath);

    string dllPath = Path.GetFullPath(inputDllPath);
    string? outputDir = !string.IsNullOrWhiteSpace(outputDirectory) ? Path.GetFullPath(outputDirectory) : null;

    if (outputDir != null)
    {
        Console.WriteLine($"Preparing IL modification of Lod.RecordCollections from '{dllPath}' to output directory '{outputDir}'");
        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        string originalDllPath = dllPath;

        // Copy the DLL, PDB, and XML to the output directory
        string outputDllPath = Path.Combine(outputDir, Path.GetFileName(originalDllPath));
        File.Copy(originalDllPath, outputDllPath, overwrite: true);

        string originalPdbPath = Path.ChangeExtension(originalDllPath, ".pdb");
        if (File.Exists(originalPdbPath))
        {
            string outputPdbPath = Path.Combine(outputDir, Path.GetFileName(originalPdbPath));
            File.Copy(originalPdbPath, outputPdbPath, overwrite: true);
        }

        string originalXmlPath = Path.ChangeExtension(originalDllPath, ".xml");
        if (File.Exists(originalXmlPath))
        {
            string outputXmlPath = Path.Combine(outputDir, Path.GetFileName(originalXmlPath));
            File.Copy(originalXmlPath, outputXmlPath, overwrite: true);
        }

        // Use the output DLL for modification
        dllPath = outputDllPath;
    }
    else
    {
        Console.WriteLine($"Preparing IL modification of Lod.RecordCollections to '{dllPath}'");
    }

    string directory = GetSearchDirectory();
    Console.WriteLine($"Searching for ilasm/ildasm in directory '{directory}'.");
    string ilasmPath = FindToolPath(directory, "ilasm.exe");
    string ildasmPath = FindToolPath(directory, "ildasm.exe");


    // Decompile
    Console.WriteLine("Decompiling IL.");

    // Use the DLL's directory for the IL file to avoid conflicts when processing multiple frameworks in parallel
    string dllDirectory = Path.GetDirectoryName(dllPath)!;
    string fileName = "Lod.RecordCollections.il";
    string ilPath = Path.Combine(dllDirectory, fileName);

    var (dasmExitCode, dasmOut, dasmErr) = await RunProcessAsync(ildasmPath, $"\"{dllPath}\" /out:\"{ilPath}\"", directory);
    if (dasmExitCode != 0)
    {
        throw new InvalidOperationException($"IL decompilation failed (exit code {dasmExitCode}).\n{dasmOut}\n{dasmErr}");
    }


    // Modify
    Console.WriteLine("Modifying IL");

    const string cloneTemplate = @"
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
    string fileContent = await File.ReadAllTextAsync(ilPath);
    IEnumerable<Type> collectionTypes = typeof(IReadOnlyRecordCollection<>).Assembly.GetTypes()
        .Where(t => t.IsClass && t.Name.StartsWith("Record") && t.IsGenericType && t.IsAssignableTo(typeof(IEnumerable)))
        .Select(t => t.GetGenericTypeDefinition());

    foreach (Type collection in collectionTypes)
    {
        string typeName = collection.FullName!;
        string shortTypeName = collection.Name; // e.g., "RecordList`1"
        MemberInfo[] existingMembers = collection.GetMember("<Clone>$", BindingFlags.Instance | BindingFlags.NonPublic);
        bool hasCloneInAssembly = existingMembers.Length > 0;

        // Check for method declaration or end marker in IL (check both full namespace and short name formats)
        // Method declaration format: "instance class System.Collections.Generic.RecordList`1<!T> '<Clone>$' ()"
        // End marker format: "} // end of method RecordList`1::'<Clone>$'" or "} // end of method System.Collections.Generic.RecordList`1::'<Clone>$'"
        // We have to check the IL itself as the assembly loaded into the AppDomain may not reflect the the change after subsequent runs of build or test tools.
        string cloneMethodSignatureFull = $"{typeName}<!T> '<Clone>$'";
        string cloneMethodSignatureShort = $"{shortTypeName}<!T> '<Clone>$'";
        string cloneMethodEndMarkerFull = $"end of method {typeName}::'<Clone>$'";
        string cloneMethodEndMarkerShort = $"end of method {shortTypeName}::'<Clone>$'";
        bool hasCloneInIl = fileContent.Contains(cloneMethodSignatureFull, StringComparison.Ordinal)
            || fileContent.Contains(cloneMethodSignatureShort, StringComparison.Ordinal)
            || fileContent.Contains(cloneMethodEndMarkerFull, StringComparison.Ordinal)
            || fileContent.Contains(cloneMethodEndMarkerShort, StringComparison.Ordinal);

        if (!hasCloneInAssembly && !hasCloneInIl)
        {
            Type[] genericArgs = collection.GetGenericArguments();
            string generics = $"<{string.Join(", ", genericArgs.Select(a => $"!{a.Name}"))}>";
            string genericIndexes = $"<{string.Join(", ", genericArgs.Select((_, i) => $"!{i}"))}>";
            string template = cloneTemplate.Replace("$!TYPE!$", typeName)
                .Replace("<!T>", generics)
                .Replace("<!0>", genericIndexes);

            string searchPattern = $"}} // end of class {typeName}";
            int replacementCount = 0;
            int lastIndex = 0;
            while ((lastIndex = fileContent.IndexOf(searchPattern, lastIndex, StringComparison.Ordinal)) != -1)
            {
                replacementCount++;
                lastIndex += searchPattern.Length;
            }

            fileContent = fileContent.Replace(searchPattern, template + "}");
        }
    }
    await File.WriteAllTextAsync(ilPath, fileContent);


    // Recompile
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
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}


static string GetSearchDirectory()
{
    // Prefer the app base directory (more deterministic/safer than CWD), fallback to the current directory.
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    return !string.IsNullOrWhiteSpace(baseDir) ? baseDir : Directory.GetCurrentDirectory();
}

static string FindToolPath(string searchDirectory, string fileName)
{
    string searchRoot = Path.GetFullPath(searchDirectory);

    string? candidate = Directory.EnumerateFiles(searchRoot, fileName, SearchOption.AllDirectories).FirstOrDefault() ?? throw new FileNotFoundException(fileName);
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