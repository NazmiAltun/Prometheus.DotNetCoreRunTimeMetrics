using System;
using System.Threading;

namespace Prometheus.NetRuntimeMetrics.Utils
{
    internal class Sampler
    {
        public static readonly Sampler Default = new Sampler(1);
        private long _next;

        public Sampler(int sampleEvery)
        {
            SampleEvery = sampleEvery;
            if (sampleEvery <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(sampleEvery)} must a positive number");
            }
        }

        public int SampleEvery { get; }

        public bool ShouldSample()
        {
            return SampleEvery switch
            {
                1 => true,
                _ => Interlocked.Increment(ref _next) % SampleEvery == 0
            };
        }
    }
}