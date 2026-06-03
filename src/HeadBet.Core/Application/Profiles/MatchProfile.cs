using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class MatchProfile : Profile
{
    public MatchProfile()
    {
        // Entity -> DTOs
        CreateMap<Match, MatchKeyDto>();

        // Entity -> ViewModels
        CreateMap<Match, MatchListViewModel>();
        CreateMap<Match, MatchFormViewModel>();
        CreateMap<Match, MatchScoreFormViewModel>();

        // FormViewModel -> Commands
        CreateMap<MatchFormViewModel, CreateMatchCommand>();
        CreateMap<MatchFormViewModel, UpdateMatchCommand>();
        CreateMap<MatchScoreFormViewModel, UpdateMatchScoreCommand>();

        // Commands -> Entity
        CreateMap<CreateMatchCommand, Match>();
        CreateMap<UpdateMatchCommand, Match>();
    }
}
