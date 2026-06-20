namespace SindarinGrammarSync;

/// <summary>
/// Localiza, por padrão, o arquivo de gramática (no próprio repo sindarin-grammar) e as fontes do
/// engine (ObjectInfos.txt + Enumerators.cs) num repositório Sindarin irmão. Tudo é sobreponível
/// por argumento de linha de comando.
/// </summary>
public static class PathResolver
{
    private const string GrammarRelative = "syntaxes/sindarin.tmLanguage.json";
    private const string ObjectInfosRelative = "src/Basic/ObjectInfos.txt";
    private const string EnumeratorsRelative = "src/Basic/Enumerators.cs";

    /// <summary>Sobe na árvore a partir de <paramref name="start"/> até achar um filho <paramref name="relativeProbe"/>.</summary>
    private static DirectoryInfo? FindUp(string start, string relativeProbe)
    {
        for (DirectoryInfo? dir = new(start); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, relativeProbe.Replace('/', Path.DirectorySeparatorChar))))
                return dir;
        }
        return null;
    }

    public static string ResolveGrammarFile(string? overridePath)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
            return Path.GetFullPath(overridePath);

        DirectoryInfo? root = FindUp(AppContext.BaseDirectory, GrammarRelative)
            ?? FindUp(Directory.GetCurrentDirectory(), GrammarRelative);
        if (root is null)
            throw new FileNotFoundException(
                $"Could not locate '{GrammarRelative}' walking up from the executable or CWD. " +
                "Pass --grammar <path>.");

        return Path.Combine(root.FullName, GrammarRelative.Replace('/', Path.DirectorySeparatorChar));
    }

    public static string ResolveSindarinRoot(string? overridePath, string grammarFile)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
            return Path.GetFullPath(overridePath);

        string? env = Environment.GetEnvironmentVariable("SINDARIN_ROOT");
        if (!string.IsNullOrWhiteSpace(env))
            return Path.GetFullPath(env);

        // A partir da raiz do repo da gramática, procura um irmão "Sindarin" em qualquer ancestral.
        string grammarRoot = Path.GetDirectoryName(Path.GetDirectoryName(grammarFile)!)!; // .../<repo>
        for (DirectoryInfo? dir = new(grammarRoot); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "Sindarin");
            if (File.Exists(Path.Combine(candidate, ObjectInfosRelative.Replace('/', Path.DirectorySeparatorChar))))
                return candidate;
        }

        throw new DirectoryNotFoundException(
            "Could not auto-detect the Sindarin repository (no sibling 'Sindarin/src/Basic/ObjectInfos.txt' " +
            "found up the tree). Pass --sindarin-root <path> or set SINDARIN_ROOT.");
    }

    public static string ObjectInfosPath(string sindarinRoot)
        => Path.Combine(sindarinRoot, ObjectInfosRelative.Replace('/', Path.DirectorySeparatorChar));

    public static string EnumeratorsPath(string sindarinRoot)
        => Path.Combine(sindarinRoot, EnumeratorsRelative.Replace('/', Path.DirectorySeparatorChar));
}
