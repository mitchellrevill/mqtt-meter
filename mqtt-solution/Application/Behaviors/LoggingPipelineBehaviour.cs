using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behavior;

public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;
        var requestGuid = Guid.NewGuid().ToString();
        var requestNameWithGuid = $"{requestName} [{requestGuid}]";
        TResponse response;

        _logger.LogInformation($"[START] {requestNameWithGuid}; Log time={DateTime.UtcNow}");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            try
            {
                _logger.LogInformation($"[PROPS] {requestNameWithGuid} {JsonSerializer.Serialize(request)}");
            }
            catch (NotSupportedException)
            {
                _logger.LogInformation($"[Serialization ERROR] {requestNameWithGuid} Could not serialize the request.");
            }

            response = await next();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"[END] {requestNameWithGuid}; Log time={DateTime.UtcNow}; Execution elapsed time={stopwatch.ElapsedMilliseconds}ms");
        }

        return response;
    }
}
