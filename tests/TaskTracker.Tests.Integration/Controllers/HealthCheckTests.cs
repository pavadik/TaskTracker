using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace TaskTracker.Tests.Integration.Controllers;

public class HealthCheckTests : IClassFixture<TaskTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(TaskTrackerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
