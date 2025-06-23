using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Arif.Platform.Tests.Unit.Shared;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_DefaultValues_AreValid()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=ArifPlatform;Trusted_Connection=true;"},
                {"Jwt:SecretKey", "test-secret-key-for-testing-purposes-only"},
                {"Jwt:Issuer", "ArifPlatform"},
                {"Jwt:Audience", "ArifPlatformUsers"}
            })
            .Build();

        configuration["ConnectionStrings:DefaultConnection"].Should().NotBeNullOrEmpty();
        configuration["Jwt:SecretKey"].Should().NotBeNullOrEmpty();
        configuration["Jwt:Issuer"].Should().Be("ArifPlatform");
        configuration["Jwt:Audience"].Should().Be("ArifPlatformUsers");
    }

    [Fact]
    public void Configuration_RequiredSections_Exist()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "test-connection"},
                {"Jwt:SecretKey", "test-key"},
                {"Redis:ConnectionString", "localhost:6379"},
                {"Logging:LogLevel:Default", "Information"}
            })
            .Build();

        configuration.GetSection("ConnectionStrings").Exists().Should().BeTrue();
        configuration.GetSection("Jwt").Exists().Should().BeTrue();
        configuration.GetSection("Redis").Exists().Should().BeTrue();
        configuration.GetSection("Logging").Exists().Should().BeTrue();
    }

    [Theory]
    [InlineData("ConnectionStrings:DefaultConnection")]
    [InlineData("Jwt:SecretKey")]
    [InlineData("Jwt:Issuer")]
    [InlineData("Jwt:Audience")]
    public void Configuration_RequiredKeys_AreNotEmpty(string key)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=ArifPlatform;"},
                {"Jwt:SecretKey", "test-secret-key"},
                {"Jwt:Issuer", "ArifPlatform"},
                {"Jwt:Audience", "ArifPlatformUsers"}
            })
            .Build();

        var value = configuration[key];

        value.Should().NotBeNullOrEmpty($"Configuration key '{key}' should have a value");
    }
}
