using System;

namespace Prometheus.NetRuntimeMetrics
{
    [Flags]
    internal enum RunTimeEventKeywords : long
    {
        GC = 1,
        Jit = 16,
        Contention = 16384,
        Exception = 32768,
        Threading = 65536,
    }
}
