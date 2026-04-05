using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InternalTicketManager.Domain.Tickets;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var existingDescriptor = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<TicketingDbContext>));

            if (existingDescriptor is not null)
            {
                services.Remove(existingDescriptor);
            }

            services.RemoveAll<TicketingDbContext>();
            services.RemoveAll<ApplicationDbInitializer>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);
            services.AddDbContext<TicketingDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<ApplicationDbInitializer>();
        });
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        var dbInitializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbInitializer.InitializeAsync();
    }

    public async Task SeedProjectAsync(Guid id, string name, string? description = null)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        dbContext.Projects.Add(new Domain.Projects.Project
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task SeedTicketAsync(
        Guid id,
        Guid projectId,
        string title,
        string createdByUsername,
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Medium,
        string? description = null,
        IReadOnlyCollection<Guid>? assignedDeveloperIds = null,
        DateTime? createdAtUtc = null,
        DateTime? updatedAtUtc = null)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        dbContext.Tickets.Add(new Ticket
        {
            Id = id,
            ProjectId = projectId,
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            CreatedByUsername = createdByUsername,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            UpdatedAtUtc = updatedAtUtc ?? createdAtUtc ?? DateTime.UtcNow
        });

        if (assignedDeveloperIds is not null)
        {
            foreach (var developerId in assignedDeveloperIds.Distinct())
            {
                dbContext.TicketAssignments.Add(new TicketAssignment
                {
                    TicketId = id,
                    UserId = developerId,
                    AssignedAtUtc = updatedAtUtc ?? createdAtUtc ?? DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<Guid> GetUserIdByUsernameAsync(string username)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        return await dbContext.Users
            .Where(user => user.Username == username)
            .Select(user => user.Id)
            .SingleAsync();
    }

    public async Task SeedCommentAsync(
        Guid id,
        Guid ticketId,
        string authorUsername,
        string content,
        DateTime? createdAtUtc = null)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        dbContext.Comments.Add(new Comment
        {
            Id = id,
            TicketId = ticketId,
            AuthorUsername = authorUsername,
            Content = content,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = CreateClient();
        await AuthenticateClientAsync(client, username, password);
        return client;
    }

    public Task<HttpClient> CreateAdminClientAsync()
    {
        return CreateAuthenticatedClientAsync("admin.demo", "AdminDemo123!");
    }

    public Task<HttpClient> CreateDeveloperClientAsync()
    {
        return CreateAuthenticatedClientAsync("developer.demo", "DeveloperDemo123!");
    }

    private static async Task AuthenticateClientAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
