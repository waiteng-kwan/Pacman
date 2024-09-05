#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Proto.Promises.Threading
{
    /// <summary>
    /// An async-compatible auto-reset event.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode, StackTraceHidden]
#endif
    public sealed partial class AsyncAutoResetEvent
    {
        /// <summary>
        /// Creates an async-compatible auto-reset event that is initially unset.
        /// </summary>
        public AsyncAutoResetEvent() : this(false)
        {
        }

        /// <summary>
        /// Creates an async-compatible auto-reset event.
        /// </summary>
        /// <param name="initialState">Whether the auto-reset event is initially set or unset.</param>
        public AsyncAutoResetEvent(bool initialState)
        {
            _isSet = initialState;
            SetCreatedStacktrace();
        }

        partial void SetCreatedStacktrace();

        /// <summary>
        /// Whether this event is currently set.
        /// </summary>
        public bool IsSet
        {
            [MethodImpl(Internal.InlineOption)]
            get => _isSet;
        }

        /// <summary>
        /// Asynchronously wait for this event to be set.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise WaitAsync()
            => WaitAsyncImpl();

        /// <summary>
        /// Asynchronously wait for this event to be set, or for the <paramref name="cancelationToken"/> to be canceled.
        /// </summary>
        /// <param name="cancelationToken">The <see cref="CancelationToken"/> used to cancel the wait.</param>
        /// <remarks>
        /// The result of the returned <see cref="Promise{T}"/> will be <see langword="true"/> if this is set before the <paramref name="cancelationToken"/> was canceled, otherwise it will be <see langword="false"/>.
        /// If this is already set, the result will be <see langword="true"/>, even if the <paramref name="cancelationToken"/> is already canceled.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public Promise<bool> TryWaitAsync(CancelationToken cancelationToken)
            => TryWaitAsyncImpl(cancelationToken);

        /// <summary>
        /// Synchronously wait for this event to be set.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public void Wait()
            => WaitImpl();

        /// <summary>
        /// Synchronously wait for this event to be set, or for the <paramref name="cancelationToken"/> to be canceled.
        /// </summary>
        /// <param name="cancelationToken">The <see cref="CancelationToken"/> used to cancel the wait.</param>
        /// <remarks>
        /// The returned value will be <see langword="true"/> if this is set before the <paramref name="cancelationToken"/> was canceled, otherwise it will be <see langword="false"/>.
        /// If this is already set, the result will be <see langword="true"/>, even if the <paramref name="cancelationToken"/> is already canceled.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public bool TryWait(CancelationToken cancelationToken)
            => TryWaitImpl(cancelationToken);

        /// <summary>
        /// Sets this event, completing a waiter.
        /// </summary>
        /// <remarks>
        /// If there are any pending waiters, this event will be reset, and a single waiter will be completed atomically.
        /// If this event is already set, this does nothing.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public void Set()
            => SetImpl();

        /// <summary>
        /// Resets this event.
        /// </summary>
        /// <remarks>
        /// If this event is already reset, this does nothing.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public void Reset()
            => ResetImpl();

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncAutoResetEvent"/> to be awaited.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CompilerServices.PromiseAwaiterVoid GetAwaiter()
            => WaitAsync().GetAwaiter();
    }
}