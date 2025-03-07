using BackendNetAD.WebAPI.Configurations;
using System.Runtime;

namespace BackendNetAD.WebAPI.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            // Mapeando los valores desde appsettings.json u otras fuentes de configuración
            services.Configure<ActiveDirectorySettings>(configuration.GetSection("ActiveDirectorySettings"));

            return services;
        }
    }
}
