using AutoMapper;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class BetProfile : Profile
{
    public BetProfile()
    {
        // Match -> BetItemViewModel
        // HomeScore / AwayScore da Match sao ignorados (sao o placar final do jogo, nao do palpite).
        // O handler preenche HomeScore / AwayScore com o palpite existente do usuario.
        CreateMap<Match, BetItemViewModel>()
            .ForMember(d => d.MatchId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.HomeScore, o => o.Ignore())
            .ForMember(d => d.AwayScore, o => o.Ignore());
    }
}
