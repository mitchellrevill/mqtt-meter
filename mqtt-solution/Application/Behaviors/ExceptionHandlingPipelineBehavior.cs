using System;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public class ExceptionHandlingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingPipelineBehavior(ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[FAIL] Unhandled exception for ${typeof(TRequest).Name}");

            throw;
        }
    }
}
