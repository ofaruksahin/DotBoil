using AutoMapper;
using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotBoil.Mapper
{
    internal class MapperModule : Module
    {
        public override Task AddModule()
        {
            var mapperProfiles = AppDomain.CurrentDomain.FindTypesWithBaseType(typeof(Profile));

            var mapperConfig = new MapperConfiguration(mc =>
            {
                foreach (var profile in mapperProfiles)
                    mc.AddProfile(profile);
            });

            var mapper = mapperConfig.CreateMapper();
            DotBoilApp.Services.TryAddSingleton(mapper);

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
