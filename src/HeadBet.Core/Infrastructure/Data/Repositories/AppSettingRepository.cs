using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class AppSettingRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<AppSetting>(context, mapper), IAppSettingRepository;
