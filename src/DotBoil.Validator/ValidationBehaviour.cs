using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Validator
{
    internal class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationBehaviour(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var validator = scope.ServiceProvider.GetService<IValidator<TRequest>>();
            if (validator is null)
                return await next();

            var validationResult = validator.Validate(request);

            if (validationResult.IsValid)
                return await next();

            throw new Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
