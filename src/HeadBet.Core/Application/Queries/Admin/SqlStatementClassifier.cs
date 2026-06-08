namespace HeadBet.Core.Application.Queries;

/// <summary>
/// Heurística simples para decidir se um comando SQL é somente-leitura
/// (caminho de consulta, executa direto) ou de escrita/DDL (caminho com
/// confirmação + transação). Classifica pela primeira palavra-chave.
/// </summary>
public static class SqlStatementClassifier
{
    private static readonly string[] READ_ONLY_PREFIXES =
        ["SELECT", "PRAGMA", "EXPLAIN", "WITH", "VALUES"];

    /// <summary>
    /// True quando o comando aparenta ser somente-leitura. <c>WITH</c> (CTE) é
    /// tratado como leitura — uma CTE seguida de INSERT/UPDATE é rara neste
    /// contexto; no pior caso o comando ainda executa pelo caminho de consulta.
    /// </summary>
    public static bool IsReadOnly(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        var firstWord = FirstKeyword(sql);
        return READ_ONLY_PREFIXES.Any(p => string.Equals(p, firstWord, StringComparison.OrdinalIgnoreCase));
    }

    private static string FirstKeyword(string sql)
    {
        var span = sql.AsSpan().TrimStart();

        // Pula comentários de linha (-- ...) no início.
        while (span.StartsWith("--"))
        {
            var newline = span.IndexOf('\n');
            if (newline < 0) return string.Empty;
            span = span[(newline + 1)..].TrimStart();
        }

        var end = 0;
        while (end < span.Length && (char.IsLetter(span[end]) || span[end] == '_'))
            end++;

        return span[..end].ToString();
    }
}
