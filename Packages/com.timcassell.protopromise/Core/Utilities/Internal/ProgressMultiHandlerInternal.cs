using Proto.Promises.Collections;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Proto.Promises
{
    partial class Internal
    {
#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal sealed class ProgressMultiHandler : ProgressBase
        {
            private TempCollectionBuilder<ProgressToken> _tokens;
            private bool _disposed;

            private ProgressMultiHandler() { }

            ~ProgressMultiHandler()
            {
                if (!_disposed)
                {
                    ReportRejection(new UnreleasedObjectException("A Progress.MultiHandler's resources were garbage collected without it being disposed."), this);
                }
            }

            [MethodImpl(InlineOption)]
            private static ProgressMultiHandler GetOrCreateCore()
            {
                var obj = ObjectPool.TryTakeOrInvalid<ProgressMultiHandler>();
                return obj == PromiseRefBase.InvalidAwaitSentinel.s_instance
                    ? new ProgressMultiHandler()
                    : obj.UnsafeAs<ProgressMultiHandler>();
            }

            internal static ProgressMultiHandler GetOrCreate()
            {
                var instance = GetOrCreateCore();
                instance._next = null;
                instance._tokens = new TempCollectionBuilder<ProgressToken>(0);
                instance._disposed = false;
                SetCreatedStacktrace(instance, 2);
                return instance;
            }

            // We use Monitor instead of SpinLocker since the lock could be held for a longer period of time when reporting.
            [MethodImpl(InlineOption)]
            private void EnterLock()
                => Monitor.Enter(this);

            [MethodImpl(InlineOption)]
            internal override void ExitLock()
                => Monitor.Exit(this);

            internal void Add(ProgressToken progressToken, int id)
            {
                if (!progressToken.HasListener)
                {
                    return;
                }
                lock (this)
                {
                    if (id != _smallFields._id)
                    {
                        throw new ObjectDisposedException("Progress.MultiHandler");
                    }
                    _tokens.Add(progressToken);
                }
            }

            internal override void Report(double value, int id)
            {
                EnterLock();
                var reportValues = new NewProgressReportValues(this, null, value, id);
                ReportCore(ref reportValues);
            }

            // ProgressMultiHandlers are reported recursively instead of iteratively, because it would be much more complex and expensive to make it iterative.
            // This type is expected to be used infrequently, so it shouldn't cause a StackOverflowException.
            internal override void Report(ref NewProgressReportValues reportValues)
            {
                // Enter this lock before exiting previous lock.
                // This prevents a race condition where another report on a separate thread could get ahead of this report.
                EnterLock();
                reportValues._reporter.ExitLock();
                ReportCore(ref reportValues);
                // Set the next to null to notify the end of the caller loop.
                reportValues._next = null;
            }

            private void ReportCore(ref NewProgressReportValues reportValues)
            {
                // Lock is already entered from the caller.
                if (reportValues._id != _smallFields._id)
                {
                    ExitLock();
                    return;
                }
                ThrowIfInPool(this);

                var reportedProgress = reportValues._value;
                for (int i = 0, max = _tokens._count; i < max; ++i)
                {
                    var token = _tokens[i];
                    // We have to hold the lock until all tokens have been reported.
                    // We enter the lock again for each listener, because each one exits the lock indiscriminately.
                    EnterLock();
                    reportValues._reporter = this;
                    reportValues._next = token._impl;
                    reportValues._value = Lerp(token._minValue, token._maxValue, reportedProgress);
                    reportValues._id = token._id;
                    do
                    {
                        reportValues._next.Report(ref reportValues);
                    } while (reportValues._next != null);
                }
                ExitLock();
            }

            internal void Dispose(int id)
            {
                lock (this)
                {
                    if (id != _smallFields._id)
                    {
                        throw new ObjectDisposedException("Progress.MultiHandler");
                    }

                    ThrowIfInPool(this);
                    _disposed = true;
                    unchecked
                    {
                        ++_smallFields._id;
                    }
                    _tokens.Dispose();
                }

                ObjectPool.MaybeRepool(this);
            }
        }
    } // class Internal
} // namespace Proto.Promises