using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comicvine.Database;

// https://medium.com/swlh/creating-a-multi-project-net-core-database-solution-a69decdf8d7e
// when running migrations, web project should be referenced as startup project
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDataServices(this IServiceCollection services, IConfiguration configuration, bool logSensitiveData = false) {
        services.AddDbContext<ComicvineContext>(o =>
                o
                    .UseNpgsql(configuration.GetConnectionString("comicvine_db"))
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging(logSensitiveData)
        );
        return services;
    }
}