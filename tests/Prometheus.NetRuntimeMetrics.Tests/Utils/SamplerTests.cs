using System;
using FluentAssertions;
using Prometheus.NetRuntimeMetrics.Utils;
using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests.Utils
{
    public class SamplerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldNotAcceptPercentageOutOfRange(int percent)
        {
            Action createSamplerAction = () => _ = new Sampler(percent);
            createSamplerAction.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(30)]
        [InlineData(100)]
        [InlineData(1)]
        public void ShouldSampleCorrectly(int sampleEvery)
        {
            var sampler = new Sampler(sampleEvery);

            for (var i = 1; i < sampleEvery; i++)
            {
                sampler.ShouldSample().Should().BeFalse();
            }

            sampler.ShouldSample().Should().BeTrue();
        }
    }
}
