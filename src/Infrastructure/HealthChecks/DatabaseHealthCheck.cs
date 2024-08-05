using Infrastructure.Database.DBContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HealthChecks;

/// <summary>
/// Health check for the database.
/// </summary>
public class SqlHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SqlHealthCheck> _logger;

    public SqlHealthCheck(ApplicationDbContext dbContext, ILogger<SqlHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Checks the health of the database executing a simple query.
    /// If the query succeeds, the health check will return healthy.
    /// If an exception occurs, the health check will return unhealthy with the exception message.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch(Exception exception)
        {
            _logger.LogError(exception, "An error occurred while checking the database health.");
            return HealthCheckResult.Unhealthy(exception.Message,  exception);
        }
    }
}