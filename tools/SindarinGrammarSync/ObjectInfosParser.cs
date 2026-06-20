namespace SindarinGrammarSync;

/// <summary>
/// Entrada de classe extraída do ObjectInfos.txt.
/// </summary>
/// <param name="Name">Nome da ObjectClass exatamente como no arquivo.</param>
/// <param name="IsProperty">true = propriedade (property-list); false = classe (class-list).</param>
public readonly record struct ObjectEntry(string Name, bool IsProperty);

/// <summary>
/// Lê o ObjectInfos.txt (fonte única das classes/propriedades do engine Sindarin).
/// A divisão em blocos espelha EXATAMENTE o ClassListGenerator (SourceGen) para evitar divergência:
/// blocos separados por ';', cada bloco com linhas [ObjectClass, Parents, IsProperty, CanMultiple, ...params].
/// </summary>
public static class ObjectInfosParser
{
    public static IReadOnlyList<ObjectEntry> Parse(string text)
    {
        var result = new List<ObjectEntry>();

        // blocks[0] é o cabeçalho de comentários antes do primeiro ';'; entradas reais a partir de 1.
        string[] blocks = text.Trim().Split(';', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < blocks.Length; i++)
        {
            List<string> lines = ParseBlock(blocks[i]);
            if (lines.Count < 4)
                continue;

            string objectClass = lines[0].Trim();
            if (string.IsNullOrWhiteSpace(objectClass))
                continue;

            if (!bool.TryParse(lines[2].Trim(), out bool isProperty))
                continue;

            result.Add(new ObjectEntry(objectClass, isProperty));
        }

        return result;
    }

    // Mesma normalização do ClassListGenerator.ParseBlock: remove linhas vazias iniciais
    // (resto do separador ';') e UMA linha vazia final.
    private static List<string> ParseBlock(string blockText)
    {
        string normalized = blockText.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = new List<string>(normalized.Split('\n'));

        while (lines.Count > 0 && lines[0].Length == 0)
            lines.RemoveAt(0);
        if (lines.Count > 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);

        return lines;
    }
}
