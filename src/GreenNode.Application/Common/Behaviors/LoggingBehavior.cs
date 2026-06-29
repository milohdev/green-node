using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        string requestName = typeof(TRequest).Name;
        _logger.LogInformation("▶ Iniciando {RequestName}", requestName);

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        TResponse response = await next();

        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            _logger.LogWarning(
                "⚠ {RequestName} tardó {ElapsedMs}ms — revisar performance",
                requestName, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation(
                "✅ {RequestName} completado en {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
