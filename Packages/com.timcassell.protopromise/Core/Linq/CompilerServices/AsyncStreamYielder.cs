using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
    public readonly partial struct AsyncStreamYielder<T> : ICriticalNotifyCompletion, Internal.IPromiseAwaiter
    {
        private readonly Internal.PromiseRefBase.AsyncEnumerableWithIterator<T> _target;
        private readonly int _enumerableId;

        [MethodImpl(Internal.InlineOption)]
        internal AsyncStreamYielder(Internal.PromiseRefBase.AsyncEnumerableWithIterator<T> target, int enumerableId)
        {
            _target = target;
            _enumerableId = enumerableId;
            CreateOverride();
        }

        static partial void CreateOverride();

#if !NETCOREAPP
        // Fix for IL2CPP not invoking the static constructor.
#if ENABLE_IL2CPP
        [MethodImpl(Internal.InlineOption)]
        static partial void CreateOverride()
#else
        static AsyncStreamYielder()
#endif
        {
            Internal.AwaitOverriderImpl<AsyncStreamYielder<T>>.Create();
        }
#endif

        /// <summary>
        /// Returns this.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public AsyncStreamYielder<T> GetAwaiter() => this;

        /// <summary>Gets whether the reader has requested the async iterator to move forward.</summary>
        /// <remarks>This property is intended for compiler use rather than use directly in code.</remarks>
        public bool IsCompleted
        {
            [MethodImpl(Internal.InlineOption)]
            get { return false; }
        }

        /// <summary>Ends the await.</summary>
        /// <remarks>This method is intended for compiler use rather than use directly in code.</remarks>
        [MethodImpl(Internal.InlineOption)]
        public void GetResult()
            => _target.GetResultForAsyncStreamYielder(_enumerableId);

        [MethodImpl(Internal.InlineOption)]
        void Internal.IPromiseAwaiter.AwaitOnCompletedInternal(Internal.PromiseRefBase asyncPromiseRef)
            => _target.AwaitOnCompletedForAsyncStreamYielder(asyncPromiseRef, _enumerableId);

        void INotifyCompletion.OnCompleted(Action continuation)
            => throw new InvalidOperationException("AsyncStreamYielder must only be used in AsyncEnumerable methods.");

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
            => throw new InvalidOperationException("AsyncStreamYielder must only be used in AsyncEnumerable methods.");

        [MethodImpl(Internal.InlineOption)]
        internal Internal.PromiseRefBase.AsyncStreamAwaiterForLinqExtension<T> ForLinqExtension()
            => new Internal.PromiseRefBase.AsyncStreamAwaiterForLinqExtension<T>(_target, _enumerableId);
    }
}