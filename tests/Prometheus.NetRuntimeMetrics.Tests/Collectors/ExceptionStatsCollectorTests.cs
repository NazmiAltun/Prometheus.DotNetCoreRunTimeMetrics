using FluentAssertions;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.NetRuntimeMetrics.Collectors;
using Prometheus.NetRuntimeMetrics.Tests.TestHelpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests.Collectors
{
    public class ExceptionStatsCollectorTests
    {
        [Fact]
        public async Task When_Exception_Occurs_Collector_Should_Count_It()
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
            DelayHelper.Delay(() => collector.ExceptionCount
                .WithLabels(typeof(TException).FullName).Value < 1);
            collector.ExceptionCount
                .WithLabels(typeof(TException).FullName).Value
                .Should().Be(1);
        }

        private ExceptionStatsCollector CreateStatsCollector()
        {
            return new ExceptionStatsCollector(new MetricFactory(new CollectorRegistry()));
        }
    }
}
