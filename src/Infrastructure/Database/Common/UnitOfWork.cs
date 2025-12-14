
using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Common;

public sealed class UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<UnitOfWork> _logger = logger;

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while saving changes to the database.");

            return Result.Failure(new Error(
                ErrorCode.Conflict,
                """
                The data may have been modified or deleted since it was last read.
                It is recommended to refresh the data and try again.
                """
            ));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while saving changes to the database.");

            string message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("UNIQUE") || message.Contains("duplicate key"))
            {
                return Result.Failure(new Error(
                    ErrorCode.Conflict,
                    "There was a conflict with the database. Please try again."
                ));
            }
            return Result.Failure(GenericErrors.SomethingWhenWrong());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving changes to the database.");
            return Result.Failure(GenericErrors.SomethingWhenWrong());
        }
    }
}
