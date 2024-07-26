using DotBoil.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotBoil.Logging
{
    internal class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestJson = await request.SerializeAsync();

            _logger.LogInformation("Request : {0}", requestJson);

            var response = await next();

            var responseJson = await response.SerializeAsync();

            _logger.LogInformation("Response : {0}", responseJson);

            return response;
        }
    }
}
