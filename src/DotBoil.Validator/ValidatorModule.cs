using DotBoil.Dependency;
using DotBoil.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotBoil.Validator
{
    internal class ValidatorModule : Module
    {
        public override string Name => "Validator";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 0;

        public override Task AddModule()
        {
            var validators = AppDomain.CurrentDomain.FindTypesWithBaseType(predicate =>
                predicate.BaseType != null &&
                predicate.BaseType.Name.Contains(typeof(AbstractValidator<>).Name));

            if (validators.Any())
            {
                foreach (var validator in validators)
                {
                    if (!string.IsNullOrEmpty(validator.Namespace) && validator.Namespace.StartsWith("Fluent"))
                        continue;

                    var validateType = validator.BaseType.GetGenericArguments().FirstOrDefault();

                    if (validateType is null)
                        continue;

                    DotBoilApp.Services.TryAddScoped(typeof(IValidator<>).MakeGenericType(validateType), validator);
                }
            }

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
