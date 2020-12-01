using System;
using Microsoft.Extensions.Logging;

namespace Prometheus.DotNetCoreRunTimeMetrics.AspNetCore
{
    internal class CollectorExceptionHandler : ICollectorExceptionHandler
    {
        private readonly ILogger<CollectorExceptionHandler> _logger;

        public CollectorExceptionHandler(ILogger<CollectorExceptionHandler> logger)
        {
            _logger = logger;
        }

        public void Handle(Exception exception)
        {
            _logger.LogError("Error occured while listening runtime events", exception);
        }
    }
}