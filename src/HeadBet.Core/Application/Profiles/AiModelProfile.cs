using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class AiModelProfile : Profile
{
    public AiModelProfile()
    {
        // Entity -> ViewModels
        CreateMap<AiModel, AiModelListViewModel>();
        CreateMap<AiModel, AiModelFormViewModel>();

        // FormViewModel -> Commands
        CreateMap<AiModelFormViewModel, CreateAiModelCommand>();
        CreateMap<AiModelFormViewModel, UpdateAiModelCommand>();

        // Commands -> Entity
        CreateMap<CreateAiModelCommand, AiModel>();
        CreateMap<UpdateAiModelCommand, AiModel>();
    }
}
