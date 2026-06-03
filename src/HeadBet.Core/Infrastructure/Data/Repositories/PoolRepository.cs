using AutoMapper;
using Headsoft.Core.Data;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadBet.Core.Infrastructure.Data.Repositories;

public sealed class PoolRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Pool>(context, mapper), IPoolRepository
{
    public async Task<List<PoolListViewModel>> ListByUserAsync(Guid userId, CancellationToken ct)
    {
        return await (
            from pool in DbSet
            join member in context.Set<PoolMember>() on pool.Id equals member.PoolId
            where member.UserId == userId
                  && (member.Status == PoolMemberStatus.Active || member.Status == PoolMemberStatus.Pending)
            select new PoolListViewModel
            {
                Id = pool.Id,
                Name = pool.Name,
                IsPaid = pool.IsPaid,
                EntryFee = pool.EntryFee,
                IsActive = pool.IsActive,
                IsPublic = pool.IsPublic,
                MyRole = member.Role,
                MyStatus = member.Status,
                MemberCount = context.Set<PoolMember>().Count(m => m.PoolId == pool.Id && m.Status == PoolMemberStatus.Active),
            }).ToListAsync(ct);
    }

    public async Task<List<PoolListViewModel>> ListPublicAvailableAsync(Guid userId, CancellationToken ct)
    {
        return await (
            from pool in DbSet
            where pool.IsPublic
                  && pool.IsActive
                  && !context.Set<PoolMember>().Any(m => m.PoolId == pool.Id && m.UserId == userId)
            select new PoolListViewModel
            {
                Id = pool.Id,
                Name = pool.Name,
                IsPaid = pool.IsPaid,
                EntryFee = pool.EntryFee,
                IsActive = pool.IsActive,
                IsPublic = pool.IsPublic,
                MyRole = default,
                MyStatus = null,
                MemberCount = context.Set<PoolMember>().Count(m => m.PoolId == pool.Id && m.Status == PoolMemberStatus.Active),
            }).ToListAsync(ct);
    }
}
