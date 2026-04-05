using InternalTicketManager.Application.Auth;
using InternalTicketManager.Application.Projects;
using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Infrastructure.Auth;
using InternalTicketManager.Infrastructure.Projects;
using InternalTicketManager.Infrastructure.Persistence;
using InternalTicketManager.Infrastructure.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InternalTicketManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TicketingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:TicketingDb is required.");
        }

        services.AddDbContext<TicketingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAuthUserStore, DatabaseAuthUserStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ApplicationDbInitializer>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }
}
