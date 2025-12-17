using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TaskTracker.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<TaskTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TaskTrackerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Password = "TestPassword123!",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeEmpty();
        content.RefreshToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        var request = new
        {
            Email = email,
            Password = "TestPassword123!",
            DisplayName = "Test User"
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Register second time with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = $"login-test-{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";
        var registerRequest = new
        {
            Email = email,
            Password = password,
            DisplayName = "Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
