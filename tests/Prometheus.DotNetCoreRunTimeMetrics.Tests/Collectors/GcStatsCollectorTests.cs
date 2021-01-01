using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests.Collectors
{
    public class GcStatsCollectorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GcStatsCollectorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void GcStatsShouldBeCollected()
        {
            using var collector = CreateStatsCollector();

            //Act
            var _ = PolluteMemory();
            ForceGc();
            //Assert
            VerifyGc(collector, "1", "Induced", "Blocking Foreground");
            VerifyGc(collector, "2", "Induced", "Blocking Foreground");
            VerifyGc(collector, "2", "Large object heap allocation", "NonBlocking Background");
            VerifyHeapSize(collector, "1");
            VerifyHeapSize(collector, "2");
            VerifyHeapSize(collector, "loh");
            VerifyLohType(collector, typeof(byte[]).FullName);
        }

        private void VerifyLohType(GcStatsCollector collector, string typeName)
        {
            collector.LargeObjectAllocationTypeTrigger.WithLabels(typeName)
                .Value.Should().BeGreaterThan(0);
        }

        private void VerifyHeapSize(GcStatsCollector collector, string gen)
        {
            collector.GcHeapSizeInBytes.WithLabels(gen)
                .Value.Should().BeGreaterThan(0);
        }

        private void VerifyGc(GcStatsCollector collector, string gen, string reason, string type)
        {
            using var gcReasonManualResetEvent = new AssertionManualResetEvent(() =>
                collector.GcReasons.WithLabels(gen, reason, type).Value > 0);
            gcReasonManualResetEvent.Wait(TimeSpan.FromSeconds(10));

            using var gcDurationManualResetEvent = new AssertionManualResetEvent(() =>
                collector.GcDuration.WithLabels(gen, reason, type).Value.Sum > 0);
            gcReasonManualResetEvent.Wait(TimeSpan.FromSeconds(10));

            collector.GcReasons.WithLabels(gen, reason, type).Value
                .Should().BeGreaterThan(0);
            collector.GcDuration.WithLabels(gen, reason, type).Value
                .Sum.Should().BeGreaterThan(0);
        }


        private void ForceGc()
        {
            GC.Collect(1);
            GC.Collect(2);
        }

        private byte[] PolluteMemory()
        {
            const int listSize = 1000;
            const int objectSizeInBytes = 1000;

            var objectList = new List<byte[]>();
            var _ = new byte[objectSizeInBytes];

            for (var i = 0; i < listSize; i++)
            {
                objectList.Add(new byte[objectSizeInBytes]);
                objectList.Add(new byte[objectSizeInBytes * 10000]);
            }

            return new byte[objectSizeInBytes];
        }

        private GcStatsCollector CreateStatsCollector()
        {
            return new GcStatsCollector(
                new MetricFactory(new CollectorRegistry()),
                new MemoryCache(new MemoryCacheOptions()),
                TestCollectorExceptionHandler.Create(_testOutputHelper),
                RuntimeStatCollectorsConfiguration.Default);
        }
    }
}