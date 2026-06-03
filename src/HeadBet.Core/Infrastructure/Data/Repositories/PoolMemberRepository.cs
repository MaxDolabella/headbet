using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class PoolMemberRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<PoolMember>(context, mapper), IPoolMemberRepository;
