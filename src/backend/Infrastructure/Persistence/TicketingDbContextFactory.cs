using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace InternalTicketManager.Infrastructure.Persistence;

public sealed class TicketingDbContextFactory : IDesignTimeDbContextFactory<TicketingDbContext>
{
    public TicketingDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Api"));

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables();

        try
        {
            var apiAssembly = Assembly.Load("InternalTicketManager.Api");
            configurationBuilder.AddUserSecrets(apiAssembly, optional: true);
        }
        catch
        {
            // Fallback to JSON/environment sources when the API assembly is unavailable at design time.
        }

        IConfigurationRoot configuration = configurationBuilder.Build();

        var connectionString = configuration.GetConnectionString("TicketingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:TicketingDb is required to create the DbContext.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TicketingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TicketingDbContext(optionsBuilder.Options);
    }
}
