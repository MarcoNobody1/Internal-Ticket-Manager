using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class ProjectsEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProjectsEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProjects_ReturnsSeededProjectsOrderedByNameAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedProjectAsync(Guid.NewGuid(), "Zulu", "Last project");
        await _factory.SeedProjectAsync(Guid.NewGuid(), "Alpha", "First project");

        var client = await _factory.CreateDeveloperClientAsync();
        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("Alpha", document.RootElement[0].GetProperty("name").GetString());
        Assert.Equal("Zulu", document.RootElement[1].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetProjectById_WhenProjectDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = await _factory.CreateDeveloperClientAsync();
        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectById_ReturnsProjectDetailsWithOnlyOpenTicketsAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        var openTicketId = Guid.NewGuid();
        var resolvedTicketId = Guid.NewGuid();
        var developerId = await _factory.GetUserIdByUsernameAsync("developer.demo");

        await _factory.SeedProjectAsync(projectId, "Platform", "Shared internal tools");
        await _factory.SeedTicketAsync(openTicketId, projectId, "Fix login form", "developer.demo", assignedDeveloperIds: [developerId]);
        await _factory.SeedTicketAsync(resolvedTicketId, projectId, "Close sprint", "developer.demo", status: Domain.Tickets.TicketStatus.Resolved);

        var client = await _factory.CreateDeveloperClientAsync();
        var response = await client.GetAsync($"/api/projects/{projectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(projectId, document.RootElement.GetProperty("id").GetGuid());
        Assert.Equal(1, document.RootElement.GetProperty("openTickets").GetArrayLength());
        Assert.Equal(openTicketId, document.RootElement.GetProperty("openTickets")[0].GetProperty("id").GetGuid());
        Assert.Equal("developer.demo", document.RootElement.GetProperty("openTickets")[0].GetProperty("assignedDevelopers")[0].GetProperty("username").GetString());
    }

    [Fact]
    public async Task PostProject_WithValidRequest_CreatesProjectAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = await _factory.CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "  Platform  ",
            description = "  Shared internal tools  "
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Platform", document.RootElement.GetProperty("name").GetString());
        Assert.Equal("Shared internal tools", document.RootElement.GetProperty("description").GetString());
        Assert.NotEqual(Guid.Empty, document.RootElement.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task PostProject_WithBlankName_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = await _factory.CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "   ",
            description = "Invalid project"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("Name").EnumerateArray(), value =>
            value.GetString() == "Name is required.");
    }

    [Fact]
    public async Task PutProject_WithExistingProject_UpdatesProjectAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform", "Old description");

        var client = await _factory.CreateAdminClientAsync();
        var response = await client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            name = "Platform Core",
            description = "  Updated description  "
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(projectId, document.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Platform Core", document.RootElement.GetProperty("name").GetString());
        Assert.Equal("Updated description", document.RootElement.GetProperty("description").GetString());
        Assert.True(document.RootElement.GetProperty("updatedAtUtc").ValueKind != JsonValueKind.Null);
    }

    [Fact]
    public async Task PutProject_WhenProjectDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = await _factory.CreateAdminClientAsync();
        var response = await client.PutAsJsonAsync($"/api/projects/{Guid.NewGuid()}", new
        {
            name = "Missing",
            description = "Project"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_WithoutBearerToken_ReturnsUnauthorizedAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostProject_WithDeveloperCredentials_ReturnsForbiddenAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = await _factory.CreateDeveloperClientAsync();
        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "Platform",
            description = "Shared internal tools"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WithAdminCredentials_DeletesProjectAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform", "Shared internal tools");

        var client = await _factory.CreateAdminClientAsync();
        var deleteResponse = await client.DeleteAsync($"/api/projects/{projectId}");
        var getResponse = await client.GetAsync($"/api/projects/{projectId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WithDeveloperCredentials_ReturnsForbiddenAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform", "Shared internal tools");

        var client = await _factory.CreateDeveloperClientAsync();
        var response = await client.DeleteAsync($"/api/projects/{projectId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
