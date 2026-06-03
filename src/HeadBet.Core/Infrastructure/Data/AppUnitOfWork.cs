using Headsoft.Core.Data;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Infrastructure.Data;

public sealed class AppUnitOfWork : UnitOfWorkBase<AppDbContext>
{
    public AppUnitOfWork(ILogger<AppUnitOfWork> logger, AppDbContext context)
        : base(logger, context)
    { }
}
