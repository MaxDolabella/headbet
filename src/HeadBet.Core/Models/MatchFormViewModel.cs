using System.ComponentModel.DataAnnotations;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Models;

public class MatchFormViewModel
{
    public Guid Id { get; set; }
    public Guid PoolId { get; set; }

    [Required(ErrorMessage = "Time da casa é obrigatório.")]
    public Guid HomeTeamId { get; set; }

    [Required(ErrorMessage = "Time visitante é obrigatório.")]
    public Guid AwayTeamId { get; set; }

    [Required(ErrorMessage = "Data/hora é obrigatória.")]
    public DateTime MatchDate { get; set; }

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    [StringLength(50, ErrorMessage = "Grupo deve ter no máximo 50 caracteres.")]
    public string? Group { get; set; }

    [Range(0, 255, ErrorMessage = "Rodada deve ser entre 0 e 255.")]
    public byte Round { get; set; }

    [StringLength(500, ErrorMessage = "Link da transmissão deve ter no máximo 500 caracteres.")]
    public string? BroadcastUrl { get; set; }

    /// <summary>Lista de times do pool para popular os dropdowns.</summary>
    public List<TeamListViewModel> AvailableTeams { get; set; } = [];
}
