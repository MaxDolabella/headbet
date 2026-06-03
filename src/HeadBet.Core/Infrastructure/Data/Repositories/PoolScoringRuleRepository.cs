using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class PoolScoringRuleRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<PoolScoringRule>(context, mapper), IPoolScoringRuleRepository;
