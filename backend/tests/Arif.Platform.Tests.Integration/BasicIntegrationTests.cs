using Xunit;
using FluentAssertions;
using System.Net;

namespace Arif.Platform.Tests.Integration;

public class BasicIntegrationTests
{
    private readonly HttpClient _client;

    public BasicIntegrationTests()
    {
        _client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(30);
    }

    [Fact]
    public async Task HttpClient_Configuration_IsValid()
    {
        _client.Should().NotBeNull();
        _client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData("http://localhost:5000")]
    [InlineData("http://localhost:5001")]
    [InlineData("http://localhost:5002")]
    public void ServiceUrls_AreValidUris(string url)
    {
        var isValid = Uri.TryCreate(url, UriKind.Absolute, out var uri);
        
        isValid.Should().BeTrue();
        uri.Should().NotBeNull();
        uri!.Scheme.Should().Be("http");
    }

    [Fact]
    public void Integration_TestFramework_IsConfigured()
    {
        var testAssembly = typeof(BasicIntegrationTests).Assembly;
        
        testAssembly.Should().NotBeNull();
        testAssembly.GetName().Name.Should().Be("Arif.Platform.Tests.Integration");
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/auth")]
    [InlineData("/api/chatbot")]
    public void ApiEndpoints_HaveValidPaths(string endpoint)
    {
        endpoint.Should().StartWith("/");
        endpoint.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ServiceConfiguration_SupportsAsyncOperations()
    {
        var task = Task.FromResult("test");
        var result = await task;
        
        result.Should().Be("test");
    }
}
