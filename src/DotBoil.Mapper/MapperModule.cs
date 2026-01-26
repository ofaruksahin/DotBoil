using AutoMapper;
using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Mapper
{
    internal class MapperModule : Module
    {
        public override Task AddModule()
        {
            var mapperProfiles = AppDomain.CurrentDomain
                .FindTypesWithBaseType(typeof(Profile))
                .Where(t => !t.IsAbstract);

            DotBoilApp.Services.AddAutoMapper(cfg =>
            {
                foreach (var profile in mapperProfiles)
                    cfg.AddProfile(profile);
            });

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
