using SindarinGrammarSync;

// -----------------------------------------------------------------------------
// SindarinGrammarSync — sincroniza as listas dinâmicas do sindarin.tmLanguage.json
// (class-list / property-list / enum-list) a partir das fontes do engine Sindarin:
//   - classes/propriedades  <- src/Basic/ObjectInfos.txt   (flag IsProperty)
//   - valores de enum        <- src/Basic/Enumerators.cs     (enum EnumeratorProperty)
// As demais seções da gramática (comment/string/number/operator/...) são estáticas.
// -----------------------------------------------------------------------------

#if DEBUG
args = [
    //"--check",
];
#endif

var options = CliOptions.Parse(args);
if (options.ShowHelp)
{
    CliOptions.PrintUsage();
    return 0;
}

try
{
    string grammarFile = PathResolver.ResolveGrammarFile(options.GrammarPath);
    string sindarinRoot = PathResolver.ResolveSindarinRoot(options.SindarinRoot, grammarFile);
    string objectInfosPath = options.ObjectInfosPath ?? PathResolver.ObjectInfosPath(sindarinRoot);
    string enumeratorsPath = options.EnumeratorsPath ?? PathResolver.EnumeratorsPath(sindarinRoot);

    if (!options.Quiet)
    {
        Console.WriteLine("SindarinGrammarSync");
        Console.WriteLine($"  grammar     : {grammarFile}");
        Console.WriteLine($"  objectInfos : {objectInfosPath}");
        Console.WriteLine($"  enumerators : {enumeratorsPath}");
        Console.WriteLine($"  mode        : {(options.Check ? "check (no write)" : "write")}");
        Console.WriteLine();
    }

    foreach (string p in new[] { grammarFile, objectInfosPath, enumeratorsPath })
        if (!File.Exists(p))
        {
            Console.Error.WriteLine($"ERROR: file not found: {p}");
            return 2;
        }

    // --- fontes -> listas (em ordem de declaração, sem duplicatas, minúsculas) ---
    IReadOnlyList<ObjectEntry> entries = ObjectInfosParser.Parse(File.ReadAllText(objectInfosPath));
    IReadOnlyList<string> enumValues = EnumeratorsParser.Parse(File.ReadAllText(enumeratorsPath));

    List<string> classNames = DistinctLower(entries.Where(e => !e.IsProperty).Select(e => e.Name));
    List<string> propertyNames = DistinctLower(entries.Where(e => e.IsProperty).Select(e => e.Name));
    List<string> enumNames = DistinctLower(enumValues);

    if (!options.Quiet)
        Console.WriteLine(
            $"Parsed: {classNames.Count} classes, {propertyNames.Count} properties, {enumNames.Count} enum values.\n");

    // --- sincroniza ---
    string grammarText = File.ReadAllText(grammarFile);
    SyncResult result = GrammarUpdater.Sync(grammarText, classNames, propertyNames, enumNames);

    foreach (SectionDiff d in result.Diffs)
        Report(d);

    if (!result.Changed)
    {
        Console.WriteLine("\nGrammar already in sync. Nothing to do.");
        return 0;
    }

    if (options.Check)
    {
        Console.WriteLine("\nDRIFT: grammar is OUT OF SYNC with the engine sources (use without --check to update).");
        return 1; // útil para CI / pre-commit
    }

    File.WriteAllText(grammarFile, result.NewText);
    Console.WriteLine($"\nGrammar updated: {grammarFile}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("ERROR: " + ex.Message);
    return 2;
}

// -----------------------------------------------------------------------------
static List<string> DistinctLower(IEnumerable<string> source)
{
    var seen = new HashSet<string>();
    var list = new List<string>();
    foreach (string s in source)
    {
        string v = s.Trim().ToLowerInvariant();
        if (v.Length > 0 && seen.Add(v))
            list.Add(v);
    }
    return list;
}

static void Report(SectionDiff d)
{
    if (!d.Changed)
    {
        Console.WriteLine($"[{d.Section}] up to date.");
        return;
    }

    Console.WriteLine($"[{d.Section}] +{d.Added.Count} -{d.Removed.Count}" + (d.Reordered ? " (reordered)" : ""));
    if (d.Added.Count > 0)
        Console.WriteLine($"    added  : {string.Join(", ", d.Added)}");
    if (d.Removed.Count > 0)
        Console.WriteLine($"    removed: {string.Join(", ", d.Removed)}");
}
