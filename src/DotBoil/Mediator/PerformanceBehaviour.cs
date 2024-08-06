using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotBoil.Mediator
{
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;

        public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var response = await next();

            stopWatch.Stop();

            _logger.LogInformation("Method {0}, Elapsed Millisecond : {0}", next.Method.Name, stopWatch.ElapsedMilliseconds);

            return response;
        }
    }
}
