using System;
using System.Threading;

namespace Prometheus.NetRuntimeMetrics.Utils
{
    internal class Sampler
    {
        public static readonly Sampler Default = new Sampler(1);
        private readonly int _sampleEvery;
        private long _next;

        public Sampler(int sampleEvery)
        {
            _sampleEvery = sampleEvery;
            if (sampleEvery <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(sampleEvery)} must a positive number");
            }
        }

        public bool ShouldSample()
        {
            return _sampleEvery switch
            {
                1 => true,
                _ => Interlocked.Increment(ref _next) % _sampleEvery == 0
            };
        }
    }
}