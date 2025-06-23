using Xunit;
using FluentAssertions;
using System.Net;

namespace Arif.Platform.Tests.Integration;

public class ApiGatewayIntegrationTests
{
    private readonly HttpClient _client;

    public ApiGatewayIntegrationTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:5000");
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task AuthenticationRoute_ReturnsUnauthorizedWithoutToken()
    {
        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsSwaggerUI()
    {
        var response = await _client.GetAsync("/swagger");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("swagger");
    }

    [Theory]
    [InlineData("/api/auth")]
    [InlineData("/api/chatbot")]
    [InlineData("/api/analytics")]
    public async Task ApiRoutes_ExistAndReturnResponse(string route)
    {
        var response = await _client.GetAsync(route);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}
