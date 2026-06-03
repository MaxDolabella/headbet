using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class PoolProfile : Profile
{
    public PoolProfile()
    {
        // Entity -> ViewModels
        CreateMap<Pool, PoolEditViewModel>()
            .ForMember(d => d.CollectedAmount, o => o.MapFrom(s => s.CollectedAmount ?? 0m));
        CreateMap<Pool, PoolListViewModel>()
            .ForMember(d => d.MemberCount, o => o.Ignore())
            .ForMember(d => d.MyRole, o => o.Ignore());
        CreateMap<Pool, PoolDetailsViewModel>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToBrt()))
            .ForMember(d => d.MyRole, o => o.Ignore())
            .ForMember(d => d.ScoringRules, o => o.Ignore())
            .ForMember(d => d.Prizes, o => o.Ignore())
            .ForMember(d => d.Members, o => o.Ignore());

        CreateMap<PoolScoringRule, ScoringRuleItemViewModel>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.ScoreType));
        CreateMap<PoolPrize, PrizeItemViewModel>()
            .ForMember(d => d.CalculatedAmount, o => o.Ignore());

        CreateMap<PoolMember, PoolMemberItemViewModel>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User!.Name))
            .ForMember(d => d.IsCurrentUser, o => o.Ignore());   // populado no handler

        // Edit ViewModel -> Command
        CreateMap<PoolEditViewModel, UpdatePoolCommand>();

        // Commands -> Entity
        CreateMap<CreatePoolCommand, Pool>()
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.InviteCode, o => o.Ignore());   // gerado no handler
        CreateMap<UpdatePoolCommand, Pool>()
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.InviteCode, o => o.Ignore());   // imutável: preserva o código existente
    }
}
