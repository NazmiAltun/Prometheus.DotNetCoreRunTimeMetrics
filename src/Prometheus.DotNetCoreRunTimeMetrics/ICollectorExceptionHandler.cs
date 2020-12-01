using System;

namespace Prometheus.DotNetCoreRunTimeMetrics
{
    public interface ICollectorExceptionHandler
    {
        void Handle(Exception exception);
    }
}