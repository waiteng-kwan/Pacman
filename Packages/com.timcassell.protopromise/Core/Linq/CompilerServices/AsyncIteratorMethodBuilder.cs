using Proto.Promises.CompilerServices;
using Proto.Promises.Linq;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Proto.Promises.CompilerServices
{
    /// <summary>
    /// Awaitable type used to wait for the consumer to move the async iterator forward.
    /// </summary>
    /// <remarks>This type is intended for compiler use rather than use directly in code.</remarks>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode, StackTraceHidden]
#endif
    public struct AsyncIteratorMethodBuilder

    {
        private AsyncPromiseMethodBuilder _builder;

        [MethodImpl(Internal.InlineOption)]
        private AsyncIteratorMethodBuilder(AsyncPromiseMethodBuilder builder)
            => _builder = builder;

        /// <summary>Gets the <see cref="AsyncIteratorMethod"/> for this builder.</summary>
        /// <returns>The <see cref="AsyncIteratorMethod"/> representing the builder's asynchronous operation.</returns>
        public AsyncIteratorMethod Task
        {
            [MethodImpl(Internal.InlineOption)]
            get { return new AsyncIteratorMethod(_builder.Task); }
        }

        /// <summary>Initializes a new <see cref="AsyncIteratorMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncIteratorMethodBuilder"/>.</returns>
        [MethodImpl(Internal.InlineOption)]
        public static AsyncIteratorMethodBuilder Create()
            => new AsyncIteratorMethodBuilder(AsyncPromiseMethodBuilder.Create());

        /// <summary>
        /// Completes the <see cref="Promise"/> in the <see cref="Promise.State">Rejected</see> state with the specified exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use to reject the promise.</param>
        public void SetException(Exception exception)
        {
            if (AsyncEnumerableDisposedException.Is(exception))
            {
                // The await foreach loop was stopped with a `break`.
                SetResult();
            }
            else
            {
                _builder.SetException(exception);
            }
        }

        /// <summary>
        /// Completes the <see cref="Promise"/> in the <see cref="Promise.State">Resolved</see> state.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public void SetResult()
            => _builder.SetResult();

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        [MethodImpl(Internal.InlineOption)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        [SecuritySafeCritical]
        [MethodImpl(Internal.InlineOption)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [MethodImpl(Internal.InlineOption)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _builder.Start(ref stateMachine);

        /// <summary>Does nothing.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        [MethodImpl(Internal.InlineOption)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
            => _builder.SetStateMachine(stateMachine);
    }
}