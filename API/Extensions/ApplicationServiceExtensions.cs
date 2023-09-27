using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(opt => {
            opt.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
