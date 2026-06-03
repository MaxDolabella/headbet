using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class TeamProfile : Profile
{
    public TeamProfile()
    {
        // Entity -> DTOs
        CreateMap<Team, TeamKeyDto>();

        // Entity -> ViewModels
        CreateMap<Team, TeamListViewModel>();
        CreateMap<Team, TeamFormViewModel>();

        // FormViewModel -> Commands
        CreateMap<TeamFormViewModel, CreateTeamCommand>();
        CreateMap<TeamFormViewModel, UpdateTeamCommand>();

        // Commands -> Entity
        CreateMap<CreateTeamCommand, Team>();
        CreateMap<UpdateTeamCommand, Team>();
    }
}
