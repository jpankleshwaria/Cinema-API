using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ApiApplication.Utility
{
    public static class AutoMapperSetup
    {

        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            AutoMapperConfig.RegisterMappings();
        }
    }

    public static class AutoMapperConfig
    {

        public static MapperConfiguration RegisterMappings()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
        }
    }
}
