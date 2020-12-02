using System;
using Xunit.Abstractions;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests
{
    public class TestCollectorExceptionHandler : ICollectorExceptionHandler
    {
        public static TestCollectorExceptionHandler Create(ITestOutputHelper testOutputHelper)
        {
            return new TestCollectorExceptionHandler(testOutputHelper);
        }

        private readonly ITestOutputHelper _testOutputHelper;

        private TestCollectorExceptionHandler(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Handle(Exception exception)
        {
            _testOutputHelper.WriteLine(exception.ToString());
        }
    }
}
