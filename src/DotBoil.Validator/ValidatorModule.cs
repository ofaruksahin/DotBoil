using DotBoil.Dependency;
using DotBoil.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Validator
{
    internal class ValidatorModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var validators = AppDomain.CurrentDomain.FindTypesWithBaseType(predicate =>
                predicate.BaseType != null &&
                predicate.BaseType.Name.Contains(typeof(AbstractValidator<>).Name));

            if (validators.Any())
            {
                foreach (var validator in validators)
                {
                    var validateType = validator.BaseType.GetGenericArguments().FirstOrDefault();

                    if (validateType is null)
                        continue;

                    builder.Services.AddScoped(typeof(IValidator<>).MakeGenericType(validateType), validator);
                }
            }

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
