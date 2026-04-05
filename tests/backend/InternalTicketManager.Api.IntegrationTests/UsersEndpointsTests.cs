using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class UsersEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public UsersEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_WithoutBearerToken_ReturnsUnauthorizedAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithDeveloperToken_ReturnsForbiddenAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "developer.demo", "DeveloperDemo123!");

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithAdminToken_ReturnsSeededUsersAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("admin.demo", document.RootElement[0].GetProperty("username").GetString());
        Assert.Equal("Admin", document.RootElement[0].GetProperty("role").GetString());
        Assert.Equal("developer.demo", document.RootElement[1].GetProperty("username").GetString());
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ReturnsNotFoundAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostUser_WithValidRequest_CreatesDeveloperUserAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            username = "  developer.ops  ",
            password = "  DeveloperOps123!  ",
            role = "Developer"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var userId = document.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, userId);
        Assert.Equal("developer.ops", document.RootElement.GetProperty("username").GetString());
        Assert.Equal("Developer", document.RootElement.GetProperty("role").GetString());

        var loginClient = _factory.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", new
        {
            username = "developer.ops",
            password = "DeveloperOps123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task PostUser_WithDuplicateUsername_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            username = "ADMIN.DEMO",
            password = "AnotherPassword123!",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("errors").GetProperty("Username").EnumerateArray(), value =>
            value.GetString() == "Username must be unique.");
    }

    [Fact]
    public async Task PutUser_WithExistingUser_UpdatesUsernameRoleAndPasswordAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            username = "release.manager",
            password = "ReleaseManager123!",
            role = "Developer"
        });

        createResponse.EnsureSuccessStatusCode();

        using var createdDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var userId = createdDocument.RootElement.GetProperty("id").GetGuid();

        var updateResponse = await client.PutAsJsonAsync($"/api/users/{userId}", new
        {
            username = "  release.admin  ",
            password = "  ReleaseAdmin123!  ",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var updatedDocument = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync());
        Assert.Equal(userId, updatedDocument.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("release.admin", updatedDocument.RootElement.GetProperty("username").GetString());
        Assert.Equal("Admin", updatedDocument.RootElement.GetProperty("role").GetString());

        var loginClient = _factory.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", new
        {
            username = "release.admin",
            password = "ReleaseAdmin123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WhenDeletingOnlyAdmin_ReturnsBadRequestAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var getAdminResponse = await client.GetAsync("/api/users");
        getAdminResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await getAdminResponse.Content.ReadAsStringAsync());
        var adminId = document.RootElement.EnumerateArray()
            .Single(user => user.GetProperty("username").GetString() == "admin.demo")
            .GetProperty("id")
            .GetGuid();

        var deleteResponse = await client.DeleteAsync($"/api/users/{adminId}");

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        using var errorDocument = JsonDocument.Parse(await deleteResponse.Content.ReadAsStringAsync());
        Assert.Contains(errorDocument.RootElement.GetProperty("errors").GetProperty("Role").EnumerateArray(), value =>
            value.GetString() == "At least one admin user is required.");
    }

    [Fact]
    public async Task DeleteUser_WithExistingDeveloper_RemovesUserAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await AuthenticateAsync(client, "admin.demo", "AdminDemo123!");

        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            username = "temp.user",
            password = "TempUser123!",
            role = "Developer"
        });

        createResponse.EnsureSuccessStatusCode();

        using var createdDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var userId = createdDocument.RootElement.GetProperty("id").GetGuid();

        var deleteResponse = await client.DeleteAsync($"/api/users/{userId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static async Task AuthenticateAsync(HttpClient client, string username, string password)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });

        loginResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
