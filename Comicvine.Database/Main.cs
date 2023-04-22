using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Comicvine.Database;

public class ComicvineContextFactory: IDesignTimeDbContextFactory<ComicvineContext>
{
    /*
     * https://stackoverflow.com/questions/72300013/integrating-entity-framework-core-into-class-library-net-standard
     * https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
     */
    public ComicvineContext CreateDbContext(string[] args) {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json", false, true)
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<ComicvineContext>();
        optionsBuilder
            .UseNpgsql(config.GetConnectionString("comicvine_db"))
            //.EnableSensitiveDataLogging() // TODO
            .UseSnakeCaseNamingConvention();
        return new ComicvineContext(optionsBuilder.Options);
    }
}