namespace HeadBet.Core.Models;

/// <summary>
/// Resultado de uma execução de SQL bruto pelo console do admin. Cobre tanto
/// consultas (<see cref="Columns"/> + <see cref="Rows"/>) quanto comandos de
/// escrita/DDL (<see cref="AffectedRows"/>).
/// </summary>
public class SqlConsoleResultViewModel
{
    /// <summary>True para SELECT/PRAGMA (resultado tabular); false para escrita/DDL.</summary>
    public bool IsQuery { get; set; }

    /// <summary>Nomes das colunas retornadas (somente consultas).</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>Linhas retornadas; cada linha é um array alinhado a <see cref="Columns"/>.</summary>
    public List<object?[]> Rows { get; set; } = [];

    /// <summary>Linhas afetadas (comandos de escrita). Null em consultas.</summary>
    public int? AffectedRows { get; set; }

    /// <summary>True quando o nº de linhas retornadas foi limitado (truncado).</summary>
    public bool Truncated { get; set; }

    /// <summary>Mensagem de erro do banco (sintaxe, constraint etc.), quando houver.</summary>
    public string? Error { get; set; }

    /// <summary>Tempo de execução em milissegundos.</summary>
    public long ElapsedMs { get; set; }
}
