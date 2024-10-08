﻿
using Domain.Interfaces;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Common;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationWriteDbContext _context;

    public UnitOfWork(ApplicationWriteDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
