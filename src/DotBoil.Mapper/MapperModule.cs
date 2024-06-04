using AutoMapper;
using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Mapper
{
    internal class MapperModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var mapperProfiles = AppDomain.CurrentDomain.FindTypesWithBaseType(typeof(Profile));

            var mapperConfig = new MapperConfiguration(mc =>
            {
                foreach (var profile in mapperProfiles)
                    mc.AddProfile(profile);
            });

            var mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
