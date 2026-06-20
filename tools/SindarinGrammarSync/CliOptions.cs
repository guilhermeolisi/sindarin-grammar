namespace SindarinGrammarSync;

/// <summary>Parsing simples dos argumentos de linha de comando.</summary>
public sealed class CliOptions
{
    public string? GrammarPath { get; private set; }
    public string? SindarinRoot { get; private set; }
    public string? ObjectInfosPath { get; private set; }
    public string? EnumeratorsPath { get; private set; }
    public bool Check { get; private set; }
    public bool Quiet { get; private set; }
    public bool ShowHelp { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        var o = new CliOptions();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help": o.ShowHelp = true; break;
                case "--check": o.Check = true; break;
                case "--quiet" or "-q": o.Quiet = true; break;
                case "--grammar": o.GrammarPath = Next(args, ref i); break;
                case "--sindarin-root": o.SindarinRoot = Next(args, ref i); break;
                case "--object-infos": o.ObjectInfosPath = Next(args, ref i); break;
                case "--enumerators": o.EnumeratorsPath = Next(args, ref i); break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    o.ShowHelp = true;
                    break;
            }
        }
        return o;
    }

    private static string Next(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for option '{args[i]}'.");
        return args[++i];
    }

    public static void PrintUsage()
    {
        Console.WriteLine(
            """
            SindarinGrammarSync — regenerate the dynamic lists in sindarin.tmLanguage.json
            from the Sindarin engine sources.

            Usage:
              SindarinGrammarSync [options]

            Options:
              --grammar <path>        tmLanguage.json to update
                                      (default: <repo>/syntaxes/sindarin.tmLanguage.json)
              --sindarin-root <path>  Sindarin repo root (default: auto-detect sibling 'Sindarin',
                                      or env SINDARIN_ROOT)
              --object-infos <path>   override ObjectInfos.txt path
              --enumerators <path>    override Enumerators.cs path
              --check                 report drift, do NOT write; exit 1 if out of sync (CI mode)
              -q, --quiet             only print the per-section diff
              -h, --help              show this help

            Sources:
              class-list / property-list  <- ObjectInfos.txt (IsProperty flag)
              enum-list                   <- Enumerators.cs (enum EnumeratorProperty)
            """);
    }
}
