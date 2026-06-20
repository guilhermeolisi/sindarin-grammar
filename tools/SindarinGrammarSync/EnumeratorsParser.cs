using System.Text.RegularExpressions;

namespace SindarinGrammarSync;

/// <summary>
/// Extrai os valores do enum <c>EnumeratorProperty</c> de Enumerators.cs.
/// É a fonte real do enum-list da gramática — os valores NÃO estão completos nos [..] do
/// ObjectInfos.txt (que ainda traz "Dumping" em vez do correto "Damping"), então o enum C# é
/// a referência confiável para o realce de literais de enumeração.
/// </summary>
public static partial class EnumeratorsParser
{
    public static IReadOnlyList<string> Parse(string sourceCode)
    {
        Match m = EnumBodyRegex().Match(sourceCode);
        if (!m.Success)
            throw new InvalidOperationException(
                "Could not find 'enum EnumeratorProperty { ... }' in Enumerators.cs.");

        string body = m.Groups[1].Value;
        var values = new List<string>();

        foreach (string raw in body.Split(','))
        {
            string token = raw.Trim();
            if (token.Length == 0)
                continue;
            // Remove eventual atribuição explícita "= valor".
            int eq = token.IndexOf('=');
            if (eq >= 0)
                token = token[..eq].Trim();
            if (token.Length > 0)
                values.Add(token);
        }

        return values;
    }

    // Casa "enum EnumeratorProperty [: byte] { CONTEÚDO }" capturando o conteúdo entre chaves.
    [GeneratedRegex(@"enum\s+EnumeratorProperty\b[^{]*\{([^}]*)\}", RegexOptions.Singleline)]
    private static partial Regex EnumBodyRegex();
}
