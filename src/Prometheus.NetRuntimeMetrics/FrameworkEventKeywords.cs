using System;

namespace Prometheus.NetRuntimeMetrics
{
    [Flags]
    internal enum FrameworkEventKeywords : long
    {
        ThreadPool = 0x0002,
    }
}