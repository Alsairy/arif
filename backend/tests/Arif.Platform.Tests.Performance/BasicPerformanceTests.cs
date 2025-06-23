using Xunit;
using FluentAssertions;
using System.Diagnostics;

namespace Arif.Platform.Tests.Performance;

public class BasicPerformanceTests
{
    [Fact]
    public void PerformanceTest_Framework_IsConfigured()
    {
        var testAssembly = typeof(BasicPerformanceTests).Assembly;
        
        testAssembly.Should().NotBeNull();
        testAssembly.GetName().Name.Should().Be("Arif.Platform.Tests.Performance");
    }

    [Fact]
    public void Stopwatch_Measurement_WorksCorrectly()
    {
        var stopwatch = Stopwatch.StartNew();
        
        Thread.Sleep(100);
        
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(90);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(5000)]
    [InlineData(10000)]
    public void Collection_Performance_MeetsExpectations(int itemCount)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var list = new List<int>();
        for (int i = 0; i < itemCount; i++)
        {
            list.Add(i);
        }
        
        stopwatch.Stop();
        
        list.Count.Should().Be(itemCount);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task AsyncOperation_Performance_IsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Delay(10));
        }
        
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void Memory_Usage_IsReasonable()
    {
        var initialMemory = GC.GetTotalMemory(true);
        
        var largeArray = new byte[1024 * 1024]; // 1MB
        Array.Fill(largeArray, (byte)1);
        
        var afterAllocation = GC.GetTotalMemory(false);
        var memoryIncrease = afterAllocation - initialMemory;
        
        memoryIncrease.Should().BeGreaterThan(1024 * 1024);
        memoryIncrease.Should().BeLessThan(2 * 1024 * 1024);
        
        largeArray = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var afterCleanup = GC.GetTotalMemory(true);
        afterCleanup.Should().BeLessThan(afterAllocation);
    }
}
