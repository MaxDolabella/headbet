using System.ComponentModel.DataAnnotations;

namespace HeadBet.Core.Models;

public class SetupTournamentFormViewModel
{
    public Guid PoolId { get; set; }

    [Required(ErrorMessage = "Nome do torneio é obrigatório.")]
    [StringLength(200, ErrorMessage = "Nome do torneio deve ter no máximo 200 caracteres.")]
    public string TournamentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione uma API Key.")]
    public Guid UserApiKeyId { get; set; }

    public List<UserApiKeyListViewModel> AvailableKeys { get; set; } = [];
}
