using Application.Abstractions.PipelineBehaviors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors;

/// <summary>
/// Behavior for logging requests and responses.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{

    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }


    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        string? traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Handling {RequestType} {@Request} TraceId:{TraceId} StartedAt:{UtcNow}",
            typeof(TRequest).Name,
            request, traceId,
            DateTime.UtcNow
         );

        Stopwatch? stopWatch = Stopwatch.StartNew();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await next();

            stopWatch.Stop();
            _logger.LogInformation("Completed {RequestType} {@Response} TraceId:{TraceId} ElapsedMs:{ElapsedMs}",
               typeof(TRequest).Name, result, traceId, stopWatch.ElapsedMilliseconds);

            return result;
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopWatch.Stop();
            _logger.LogWarning(
                "Cancelled {RequestType} TraceId:{TraceId} ElapsedMs:{ElapsedMs}",
                typeof(TRequest).Name,
                traceId,
                stopWatch.ElapsedMilliseconds
            );
            throw;
        }
        catch (Exception ex)
        {
            stopWatch.Stop();
            _logger.LogError(ex, "Error handling {RequestType} TraceId:{TraceId} ElapsedMs:{ElapsedMs}",
                typeof(TRequest).Name, traceId, stopWatch.ElapsedMilliseconds);
            throw;
        }
    }
}
