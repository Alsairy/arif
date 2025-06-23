using Xunit;
using FluentAssertions;

namespace Arif.Platform.Tests.Unit;

public class BasicTests
{
    [Fact]
    public void BasicTest_Always_Passes()
    {
        var expected = true;

        var actual = true;

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void Add_TwoNumbers_ReturnsSum(int a, int b, int expected)
    {
        var result = a + b;

        result.Should().Be(expected);
    }

    [Fact]
    public void String_Concatenation_WorksCorrectly()
    {
        var first = "Hello";
        var second = "World";

        var result = $"{first} {second}";

        result.Should().Be("Hello World");
    }

    [Fact]
    public void Guid_Generation_CreatesUniqueValues()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        guid1.Should().NotBe(guid2);
        guid1.Should().NotBe(Guid.Empty);
        guid2.Should().NotBe(Guid.Empty);
    }
}
