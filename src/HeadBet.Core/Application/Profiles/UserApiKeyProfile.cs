using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class UserApiKeyProfile : Profile
{
    public UserApiKeyProfile()
    {
        // Entity -> ViewModels
        CreateMap<UserApiKey, UserApiKeyListViewModel>()
            .ForMember(vm => vm.MaskedKey, opt => opt.MapFrom(e => MaskKey(e.ApiKey)));

        // Entity -> Agent DTO (flatten: AiModel.Provider, AiModel.Name)
        CreateMap<UserApiKey, UserApiKeyAgentDto>();

        // Commands -> Entity
        CreateMap<CreateUserApiKeyCommand, UserApiKey>();
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length <= 8)
            return "****";

        return string.Concat(key.AsSpan(0, 4), "****", key.AsSpan(key.Length - 4));
    }
}
