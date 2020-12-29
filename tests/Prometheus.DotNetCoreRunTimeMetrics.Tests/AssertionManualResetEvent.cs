using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prometheus.DotNetCoreRunTimeMetrics.Tests
{
    public class AssertionManualResetEvent : IDisposable
    {
        private readonly ManualResetEventSlim _innerResetEventSlim = new ManualResetEventSlim(false);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _disposed;

        public AssertionManualResetEvent(Func<bool> setConditionFunc)
        {
            _ = Task.Run(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (setConditionFunc())
                    {
                        _innerResetEventSlim.Set();
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void Wait(TimeSpan timeout)
        {
            _cancellationTokenSource.CancelAfter(timeout);
            _innerResetEventSlim.Wait(timeout);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _innerResetEventSlim?.Dispose();
                _cancellationTokenSource?.Dispose();
            }

            _disposed = true;
        }
    }
}
