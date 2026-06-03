using AutoMapper;
using HeadBet.Core.Application.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Profiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        // Entity -> ViewModels (nunca expor PasswordHash)
        CreateMap<User, UserListViewModel>();
        CreateMap<User, UserFormViewModel>()
            .ForMember(vm => vm.Password, opt => opt.Ignore());
        CreateMap<User, ProfileViewModel>();

        // FormViewModel -> Commands
        CreateMap<UserFormViewModel, CreateUserCommand>();
        CreateMap<UserFormViewModel, UpdateUserCommand>();

        // Create command -> Entity (PasswordHash e setado manualmente no handler via IPasswordHasher)
        CreateMap<CreateUserCommand, User>()
            .ForMember(u => u.PasswordHash, opt => opt.Ignore());
    }
}
