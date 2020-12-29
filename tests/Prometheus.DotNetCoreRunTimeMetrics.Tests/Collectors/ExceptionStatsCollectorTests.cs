using System;
using FluentAssertions;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class ExceptionStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ExceptionStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void When_Exception_Occurs_Collector_Should_Count_It()
        {
            using var collector = CreateStatsCollector();

            try
            {
                var divider = 0;
                _ = 1 / divider;
            }
            catch
            {
            }

            ExceptionShouldBeCollected<DivideByZeroException>(collector);
        }

        private void ExceptionShouldBeCollected<TException>(ExceptionStatsCollector collector)
            where TException : Exception
        {
            using var assertionManualResetEvent = new AssertionManualResetEvent(() => 
                collector.ExceptionCount.WithLabels(typeof(TException).FullName).Value >= 1);
            assertionManualResetEvent.Wait(TimeSpan.FromSeconds(10));
            collector.ExceptionCount
                .WithLabels(typeof(TException).FullName).Value
                .Should().Be(1);
        }

        private ExceptionStatsCollector CreateStatsCollector()
        {
            return new ExceptionStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                TestCollectorExceptionHandler.Create(_testOutputHelper));
        }
    }
}
