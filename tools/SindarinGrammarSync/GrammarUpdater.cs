using System.Text.Json;
using System.Text.RegularExpressions;

namespace SindarinGrammarSync;

/// <summary>Resultado da sincronização de UMA seção (class-list / property-list / enum-list).</summary>
public sealed class SectionDiff
{
    public required string Section { get; init; }
    public required IReadOnlyList<string> Added { get; init; }
    public required IReadOnlyList<string> Removed { get; init; }
    public bool Changed => Added.Count > 0 || Removed.Count > 0 || Reordered;
    /// <summary>true quando o conjunto é igual mas a ordem mudou.</summary>
    public bool Reordered { get; init; }
}

public sealed class SyncResult
{
    public required string NewText { get; init; }
    public required IReadOnlyList<SectionDiff> Diffs { get; init; }
    public bool Changed { get; init; }
}

/// <summary>
/// Reescreve apenas as três alternâncias (class-list / property-list / enum-list) dentro do
/// sindarin.tmLanguage.json, preservando todo o resto do arquivo (formatação, demais seções).
/// Edição por substituição de texto = diff mínimo; o resultado é validado como JSON antes de aceitar.
/// </summary>
public static class GrammarUpdater
{
    // Cada seção tem a forma:  "<key>": { "name": "...", "match": "(?i)(?:\\b)(LISTA)(?:\\b)" }
    private static Regex SectionRegex(string key) => new(
        "(\"" + Regex.Escape(key) + "\"\\s*:\\s*\\{\\s*\"name\"\\s*:\\s*\"[^\"]*\"\\s*,\\s*\"match\"\\s*:\\s*\")([^\"]*)(\")",
        RegexOptions.Singleline);

    public static SyncResult Sync(
        string grammarText,
        IReadOnlyList<string> classNames,
        IReadOnlyList<string> propertyNames,
        IReadOnlyList<string> enumNames)
    {
        var diffs = new List<SectionDiff>();
        string text = grammarText;

        text = SyncSection(text, "class-list", classNames, diffs);
        text = SyncSection(text, "property-list", propertyNames, diffs);
        text = SyncSection(text, "enum-list", enumNames, diffs);

        bool changed = !ReferenceEquals(text, grammarText) && text != grammarText;

        if (changed)
        {
            // Sanidade: o resultado tem que continuar sendo JSON válido. Se não for, aborta.
            try { using var _ = JsonDocument.Parse(text); }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Generated grammar is not valid JSON — aborting without writing. " + ex.Message);
            }
        }

        return new SyncResult { NewText = text, Diffs = diffs, Changed = changed };
    }

    private static string SyncSection(
        string text, string key, IReadOnlyList<string> names, List<SectionDiff> diffs)
    {
        Regex rx = SectionRegex(key);
        Match m = rx.Match(text);
        if (!m.Success)
            throw new InvalidOperationException(
                $"Section \"{key}\" not found in grammar (expected a \"name\"+\"match\" pair).");

        string oldValue = m.Groups[2].Value;
        IReadOnlyList<string> oldNames = ExtractNames(oldValue);
        string newValue = BuildMatchValue(names);

        var oldSet = new HashSet<string>(oldNames);
        var newSet = new HashSet<string>(names);
        var added = names.Where(n => !oldSet.Contains(n)).ToList();
        var removed = oldNames.Where(n => !newSet.Contains(n)).ToList();
        bool reordered = added.Count == 0 && removed.Count == 0 && !oldNames.SequenceEqual(names);

        diffs.Add(new SectionDiff
        {
            Section = key,
            Added = added,
            Removed = removed,
            Reordered = reordered,
        });

        if (oldValue == newValue)
            return text; // nada a fazer nesta seção

        return text[..m.Groups[2].Index] + newValue + text[(m.Groups[2].Index + m.Groups[2].Length)..];
    }

    /// <summary>
    /// Monta o valor do "match" exatamente no estilo já usado pela gramática:
    /// <c>(?i)(?:\b)(n1|n2|...)(?:\b)</c>. Como o JSON escapa a barra invertida, no arquivo o
    /// <c>\b</c> aparece como <c>\\b</c> — por isso "\\\\b" (4 barras no código → 2 no arquivo).
    /// </summary>
    private static string BuildMatchValue(IReadOnlyList<string> names)
        => "(?i)(?:\\\\b)(" + string.Join("|", names) + ")(?:\\\\b)";

    // Extrai a lista de nomes da alternância existente. Os nomes são só [a-z0-9], então o único
    // grupo "(...)" composto apenas dessas letras e de '|' é o da lista (os (?i)/(?:\\b) têm '?'/'\').
    private static IReadOnlyList<string> ExtractNames(string matchValue)
    {
        Match m = Regex.Match(matchValue, @"\(([a-z0-9|]+)\)");
        if (!m.Success)
            return [];
        return m.Groups[1].Value.Split('|', StringSplitOptions.RemoveEmptyEntries);
    }
}
