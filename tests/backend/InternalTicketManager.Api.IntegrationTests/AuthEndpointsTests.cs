using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using InternalTicketManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsTokenAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin.demo",
            password = "AdminDemo123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("accessToken").GetString()));
        Assert.Equal("admin.demo", document.RootElement.GetProperty("username").GetString());
        Assert.Equal("Admin", document.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin.demo",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutBearerToken_ReturnsUnauthorizedAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeveloperLogin_TokenContainsRoleClaim_AndMeEchoesIdentityAsync()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "developer.demo",
            password = "DeveloperDemo123!"
        });

        loginResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        Assert.Equal("developer.demo", jwt.Subject);
        Assert.Equal(
            "Developer",
            jwt.Claims.First(claim => claim.Type == "role" || claim.Type == ClaimTypes.Role).Value);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using var meDocument = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.True(meDocument.RootElement.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal("developer.demo", meDocument.RootElement.GetProperty("username").GetString());
        Assert.Equal("Developer", meDocument.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task DatabaseInitialization_SeedsExpectedUsersAndRolesAsync()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        Assert.Equal(2, await dbContext.Roles.CountAsync());
        Assert.Equal(2, await dbContext.Users.CountAsync());

        var adminUser = await dbContext.Users.Include(user => user.Role).SingleAsync(user => user.Username == "admin.demo");
        Assert.Equal("Admin", adminUser.Role.Name);
        Assert.DoesNotContain("AdminDemo123!", adminUser.PasswordHash, StringComparison.Ordinal);
    }
}
