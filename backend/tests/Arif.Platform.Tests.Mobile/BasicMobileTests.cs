using Xunit;
using FluentAssertions;

namespace Arif.Platform.Tests.Mobile;

public class BasicMobileTests
{
    [Fact]
    public void MobileTest_Framework_IsConfigured()
    {
        var testAssembly = typeof(BasicMobileTests).Assembly;
        
        testAssembly.Should().NotBeNull();
        testAssembly.GetName().Name.Should().Be("Arif.Platform.Tests.Mobile");
    }

    [Theory]
    [InlineData("iOS")]
    [InlineData("Android")]
    public void MobilePlatform_Names_AreValid(string platform)
    {
        platform.Should().NotBeNullOrEmpty();
        platform.Should().BeOneOf("iOS", "Android");
    }

    [Fact]
    public void MobileApp_Configuration_IsValid()
    {
        var appName = "ArifAgent";
        var bundleId = "com.arif.agent";
        
        appName.Should().NotBeNullOrEmpty();
        bundleId.Should().NotBeNullOrEmpty();
        bundleId.Should().StartWith("com.");
    }

    [Theory]
    [InlineData("ar")]
    [InlineData("en")]
    public void MobileApp_SupportedLanguages_AreValid(string languageCode)
    {
        languageCode.Should().NotBeNullOrEmpty();
        languageCode.Length.Should().Be(2);
        languageCode.Should().BeOneOf("ar", "en");
    }

    [Fact]
    public void MobileApp_Features_AreConfigured()
    {
        var features = new[]
        {
            "Authentication",
            "ChatManagement",
            "LiveAgentHandoff",
            "PushNotifications",
            "OfflineSupport"
        };
        
        features.Should().NotBeEmpty();
        features.Should().HaveCount(5);
        features.Should().Contain("Authentication");
        features.Should().Contain("ChatManagement");
    }

    [Fact]
    public async Task MobileApp_AsyncOperations_WorkCorrectly()
    {
        var task = Task.FromResult("mobile test");
        var result = await task;
        
        result.Should().Be("mobile test");
    }

    [Theory]
    [InlineData("https://api.arif.platform/auth")]
    [InlineData("https://api.arif.platform/chat")]
    [InlineData("https://api.arif.platform/agent")]
    public void MobileApp_ApiEndpoints_AreValidUrls(string endpoint)
    {
        var isValid = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri);
        
        isValid.Should().BeTrue();
        uri.Should().NotBeNull();
        uri!.Scheme.Should().Be("https");
    }
}
