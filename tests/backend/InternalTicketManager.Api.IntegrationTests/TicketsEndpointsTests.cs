using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InternalTicketManager.Domain.Tickets;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class TicketsEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TicketsEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTickets_ReturnsSeededTicketsOrderedByCreatedAtDescendingAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");
        await _factory.SeedTicketAsync(
            Guid.NewGuid(),
            projectId,
            "Older ticket",
            "marco",
            createdAtUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
            updatedAtUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        await _factory.SeedTicketAsync(
            Guid.NewGuid(),
            projectId,
            "Newer ticket",
            "marco",
            createdAtUtc: new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc),
            updatedAtUtc: new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc));

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("Newer ticket", document.RootElement[0].GetProperty("title").GetString());
        Assert.Equal("Older ticket", document.RootElement[1].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetTicketById_WhenTicketDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/tickets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_WhenTicketExists_ReturnsCommentsOrderedByCreatedAtAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        await _factory.SeedProjectAsync(projectId, "Platform");
        await _factory.SeedTicketAsync(ticketId, projectId, "Fix login form", "marco");
        await _factory.SeedCommentAsync(
            Guid.NewGuid(),
            ticketId,
            "developer.demo",
            "Second comment",
            new DateTime(2026, 4, 5, 10, 30, 0, DateTimeKind.Utc));
        await _factory.SeedCommentAsync(
            Guid.NewGuid(),
            ticketId,
            "admin.demo",
            "First comment",
            new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc));

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/tickets/{ticketId}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("First comment", document.RootElement[0].GetProperty("content").GetString());
        Assert.Equal("Second comment", document.RootElement[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task GetComments_WhenTicketDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/tickets/{Guid.NewGuid()}/comments");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostTicket_WithValidRequest_CreatesTicketAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "  Fix login form  ",
            description = "  The submit button stays disabled.  ",
            status = TicketStatus.Open,
            priority = TicketPriority.High,
            projectId,
            assignedUserId = "  dev-01  ",
            createdByUsername = "  marco  "
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Fix login form", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("The submit button stays disabled.", document.RootElement.GetProperty("description").GetString());
        Assert.Equal(projectId, document.RootElement.GetProperty("projectId").GetGuid());
        Assert.Equal("dev-01", document.RootElement.GetProperty("assignedUserId").GetString());
        Assert.Equal("marco", document.RootElement.GetProperty("createdByUsername").GetString());
        Assert.Equal((int)TicketPriority.High, document.RootElement.GetProperty("priority").GetInt32());
    }

    [Fact]
    public async Task PostComment_WithValidRequest_CreatesCommentAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        await _factory.SeedProjectAsync(projectId, "Platform");
        await _factory.SeedTicketAsync(ticketId, projectId, "Fix login form", "marco");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new
        {
            authorUsername = "  developer.demo  ",
            content = "  I can reproduce this issue on Firefox too.  "
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(ticketId, document.RootElement.GetProperty("ticketId").GetGuid());
        Assert.Equal("developer.demo", document.RootElement.GetProperty("authorUsername").GetString());
        Assert.Equal("I can reproduce this issue on Firefox too.", document.RootElement.GetProperty("content").GetString());
    }

    [Fact]
    public async Task PostComment_WithBlankContent_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        await _factory.SeedProjectAsync(projectId, "Platform");
        await _factory.SeedTicketAsync(ticketId, projectId, "Fix login form", "marco");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new
        {
            authorUsername = "developer.demo",
            content = "   "
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("Content").EnumerateArray(), value =>
            value.GetString() == "Content is required.");
    }

    [Fact]
    public async Task PostComment_WhenTicketDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/tickets/{Guid.NewGuid()}/comments", new
        {
            authorUsername = "developer.demo",
            content = "I can reproduce this issue."
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostTicket_WithBlankTitle_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "   ",
            description = "Invalid ticket",
            status = TicketStatus.Open,
            priority = TicketPriority.Medium,
            projectId,
            createdByUsername = "marco"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("Title").EnumerateArray(), value =>
            value.GetString() == "Title is required.");
    }

    [Fact]
    public async Task PostTicket_WithBlankCreatedByUsername_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Fix login form",
            description = "Invalid ticket",
            status = TicketStatus.Open,
            priority = TicketPriority.Medium,
            projectId,
            createdByUsername = "   "
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("CreatedByUsername").EnumerateArray(), value =>
            value.GetString() == "CreatedByUsername is required.");
    }

    [Fact]
    public async Task PostTicket_WhenProjectDoesNotExist_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Fix login form",
            description = "Project missing",
            status = TicketStatus.Open,
            priority = TicketPriority.Medium,
            projectId = Guid.NewGuid(),
            createdByUsername = "marco"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("ProjectId").EnumerateArray(), value =>
            value.GetString() == "Project does not exist.");
    }

    [Fact]
    public async Task PutTicket_WithExistingTicket_UpdatesTicketAsync()
    {
        await _factory.ResetDatabaseAsync();
        var currentProjectId = Guid.NewGuid();
        var newProjectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        await _factory.SeedProjectAsync(currentProjectId, "Platform");
        await _factory.SeedProjectAsync(newProjectId, "Support");
        await _factory.SeedTicketAsync(
            ticketId,
            currentProjectId,
            "Old title",
            "marco",
            description: "Old description",
            assignedUserId: "dev-01",
            createdAtUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
            updatedAtUtc: new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));

        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", new
        {
            title = "Updated title",
            description = "  Updated description  ",
            status = TicketStatus.InProgress,
            priority = TicketPriority.Critical,
            projectId = newProjectId,
            assignedUserId = "  dev-02  "
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(ticketId, document.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Updated title", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("Updated description", document.RootElement.GetProperty("description").GetString());
        Assert.Equal(newProjectId, document.RootElement.GetProperty("projectId").GetGuid());
        Assert.Equal("dev-02", document.RootElement.GetProperty("assignedUserId").GetString());
        Assert.Equal("marco", document.RootElement.GetProperty("createdByUsername").GetString());
        Assert.Equal(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc), document.RootElement.GetProperty("createdAtUtc").GetDateTime());
        Assert.True(document.RootElement.GetProperty("updatedAtUtc").GetDateTime() > document.RootElement.GetProperty("createdAtUtc").GetDateTime());
    }

    [Fact]
    public async Task PutTicket_WhenTicketDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");

        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/tickets/{Guid.NewGuid()}", new
        {
            title = "Updated title",
            description = "Updated description",
            status = TicketStatus.InProgress,
            priority = TicketPriority.High,
            projectId,
            assignedUserId = "dev-02"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTicket_WhenProjectDoesNotExist_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        await _factory.SeedProjectAsync(projectId, "Platform");
        await _factory.SeedTicketAsync(ticketId, projectId, "Old title", "marco");

        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", new
        {
            title = "Updated title",
            description = "Updated description",
            status = TicketStatus.InProgress,
            priority = TicketPriority.High,
            projectId = Guid.NewGuid(),
            assignedUserId = "dev-02"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("ProjectId").EnumerateArray(), value =>
            value.GetString() == "Project does not exist.");
    }
}
