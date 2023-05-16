﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comicvine.Database;

// https://medium.com/swlh/creating-a-multi-project-net-core-database-solution-a69decdf8d7e
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDataServices(this IServiceCollection services, IConfiguration configuration) {
        services.AddDbContext<ComicvineContext>(o =>
            o
            .UseNpgsql(configuration.GetConnectionString("comicvine_db"))
            .UseSnakeCaseNamingConvention()
        );
        return services;
    }
}