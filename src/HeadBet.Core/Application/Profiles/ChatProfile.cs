using AutoMapper;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class ChatProfile : Profile
{
    public ChatProfile()
    {
        // ChatMessage -> ChatMessageViewModel
        // UserName vem da navegação User (ProjectTo traduz o join no SQL).
        // CreatedAt é convertido para BRT no handler (ToBrt não é traduzível em projeção).
        CreateMap<ChatMessage, ChatMessageViewModel>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.Name));
    }
}
