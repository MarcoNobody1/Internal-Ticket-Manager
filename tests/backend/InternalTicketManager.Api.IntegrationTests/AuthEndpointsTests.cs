using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InternalTicketManager.Api.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsTokenAsync()
    {
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
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeveloperLogin_TokenContainsRoleClaim_AndMeEchoesIdentityAsync()
    {
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
}
