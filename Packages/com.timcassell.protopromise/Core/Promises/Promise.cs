﻿// define PROTO_PROMISE_DEBUG_ENABLE to enable debugging options in RELEASE mode. define PROTO_PROMISE_DEBUG_DISABLE to disable debugging options in DEBUG mode.
#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Proto.Promises
{
    /// <summary>
    /// A <see cref="Promise"/> represents the eventual result of an asynchronous operation.
    /// The primary ways of interacting with a <see cref="Promise"/> are via the `await` keyword in an async function,
    /// or through its then method, which registers callbacks to be invoked when the <see cref="Promise"/> is resolved,
    /// or the reason why the <see cref="Promise"/> cannot be resolved.
    /// </summary>
    public readonly partial struct Promise : IEquatable<Promise>
    {
#if UNITY_2021_2_OR_NEWER || !UNITY_2018_3_OR_NEWER
        /// <summary>
        /// Convert this to a <see cref="System.Threading.Tasks.ValueTask"/>.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public System.Threading.Tasks.ValueTask AsValueTask()
        {
            ValidateOperation(1);
            var r = _ref;
            return r == null
                ? new System.Threading.Tasks.ValueTask()
                : r.ToValueTaskVoid(_id);
        }

        /// <summary>
        /// Cast to <see cref="System.Threading.Tasks.ValueTask"/>.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public static implicit operator System.Threading.Tasks.ValueTask(in Promise rhs)
            => rhs.AsValueTask();
#endif // UNITY_2021_2_OR_NEWER || !UNITY_2018_3_OR_NEWER

        /// <summary>
        /// Gets whether this instance is valid to be awaited.
        /// </summary>
        public bool IsValid
            // I would prefer to have a null ref only valid if the promise was created from Promise.Resolved, but it's more efficient to allow default values to be valid.
            => _ref?.GetIsValid(_id) != false;

        /// <summary>
        /// Mark this as awaited and get a new <see cref="Promise"/> that inherits the state of this and can be awaited multiple times until <see cref="Forget"/> is called on it.
        /// <para/><see cref="Forget"/> must be called when you are finished with it.
        /// <para/>NOTE: You should not return a preserved <see cref="Promise"/> from a public API. Use <see cref="Duplicate"/> to get a <see cref="Promise"/> that is publicly safe.
        /// </summary>
        /// <remarks>This method is obsolete. You should instead use <see cref="GetRetainer"/>.</remarks>
        [Obsolete("Prefer Promise.GetRetainer()", false), EditorBrowsable(EditorBrowsableState.Never)]
        public Promise Preserve()
        {
            ValidateOperation(1);
            var r = _ref;
            if (r == null)
            {
                return this;
            }
            var newPromise = Internal.PromiseRefBase.PromiseMultiAwait<Internal.VoidResult>.GetOrCreateAndHookup(r, _id);
            return new Promise(newPromise, newPromise.Id);
        }

        /// <summary>
        /// Mark this as awaited and prevent any further awaits or callbacks on this.
        /// <para/>NOTE: It is imperative to terminate your promise chains with Forget so that any uncaught rejections will be reported and objects repooled (if pooling is enabled).
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public void Forget()
        {
            ValidateOperation(1);
            _ref?.Forget(_id);
        }

        /// <summary>
        /// Mark this as awaited and wait for the operation to complete.
        /// If the operation was rejected or canceled, the appropriate exception will be thrown.
        /// </summary>
        /// <remarks>Warning: this may cause a deadlock if you are not careful. Make sure you know what you are doing!</remarks>
        public void Wait()
            => WaitNoThrow().RethrowIfRejectedOrCanceled();

        /// <summary>
        /// Mark this as awaited and wait for the operation to complete, without throwing. Returns a <see cref="ResultContainer"/> that wraps the completion state and reason.
        /// </summary>
        /// <remarks>Warning: this may cause a deadlock if you are not careful. Make sure you know what you are doing!</remarks>
        public ResultContainer WaitNoThrow()
        {
            ValidateOperation(1);
            var r = _ref;
            if (r == null)
            {
                return new ResultContainer(null, State.Resolved);
            }
            Internal.PromiseSynchronousWaiter.TryWaitForResult(r, _id, TimeSpan.FromMilliseconds(Timeout.Infinite), out var resultContainer);
            return resultContainer;
        }

        /// <summary>
        /// Mark this as awaited and wait for the operation to complete with a specified timeout.
        /// <para/>This will return <see langword="true"/> if the operation completed successfully before the timeout expired, <see langword="false"/> otherwise.
        /// If the operation was rejected or canceled, the appropriate exception will be thrown.
        /// </summary>
        /// <remarks>
        /// If a <see cref="TimeSpan"/> representing -1 millisecond is specified for the timeout parameter, this method blocks indefinitely until the operation is complete.
        /// <para/>Warning: this may cause a deadlock if you are not careful. Make sure you know what you are doing!
        /// </remarks>
        public bool TryWait(TimeSpan timeout)
        {
            if (!TryWaitNoThrow(timeout, out var resultContainer))
            {
                return false;
            }
            resultContainer.RethrowIfRejectedOrCanceled();
            return true;
        }

        /// <summary>
        /// Mark this as awaited and wait for the operation to complete with a specified timeout, without throwing. <paramref name="resultContainer"/> wraps the completion state and reason.
        /// <para/>This will return <see langword="true"/> if the operation completed successfully before the timeout expired, <see langword="false"/> otherwise.
        /// </summary>
        /// <remarks>
        /// If a <see cref="TimeSpan"/> representing -1 millisecond is specified for the timeout parameter, this method blocks indefinitely until the operation is complete.
        /// <para/>Warning: this may cause a deadlock if you are not careful. Make sure you know what you are doing!
        /// </remarks>
        public bool TryWaitNoThrow(TimeSpan timeout, out ResultContainer resultContainer)
        {
            ValidateOperation(1);
            var r = _ref;
            if (r == null)
            {
                resultContainer = new ResultContainer(null, State.Resolved);
                return true;
            }
            return Internal.PromiseSynchronousWaiter.TryWaitForResult(r, _id, timeout, out resultContainer);
        }


        /// <summary>
        /// Mark this as awaited and get a new <see cref="Promise"/> that inherits the state of this and can be awaited once.
        /// <para/>Preserved promises are unsafe to return from public APIs. Use <see cref="Duplicate"/> to get a <see cref="Promise"/> that is publicly safe.
        /// <para/><see cref="Duplicate"/> is safe to call even if you are unsure if this is preserved.
        /// </summary>
        /// <remarks>This method is obsolete. You should instead use <see cref="GetRetainer"/> with <see cref="Retainer.WaitAsync"/>.</remarks>
        [Obsolete("Prefer Promise.Retainer.WaitAsync()", false), EditorBrowsable(EditorBrowsableState.Never)]
        public Promise Duplicate()
        {
            ValidateOperation(1);
            return Internal.PromiseRefBase.CallbackHelperVoid.Duplicate(this);
        }

        /// <summary>
        /// Mark this as awaited and schedule the next continuation to execute on the context of the provided option.
        /// Returns a new <see cref="Promise"/> that inherits the state of this, or will be canceled if/when the <paramref name="cancelationToken"/> is canceled before this is complete.
        /// </summary>
        /// <param name="continuationOption">Indicates on which context the next continuation will be executed.</param>
        /// <param name="forceAsync">If true, forces the next continuation to be invoked asynchronously. If <paramref name="continuationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
        /// <param name="cancelationToken">If canceled before this is complete, the returned <see cref="Promise"/> will be canceled, and the cancelation will propagate on the context of the provided <paramref name="continuationOption"/>.</param>
        [MethodImpl(Internal.InlineOption)]
        public Promise WaitAsync(SynchronizationOption continuationOption, bool forceAsync = false, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            return Internal.PromiseRefBase.CallbackHelperVoid.WaitAsync(this, (Internal.SynchronizationOption) continuationOption, null, forceAsync, cancelationToken);
        }

        /// <summary>
        /// Mark this as awaited and schedule the next continuation to execute on <paramref name="continuationContext"/>.
        /// Returns a new <see cref="Promise"/> that inherits the state of this, or will be canceled if/when the <paramref name="cancelationToken"/> is canceled before this is complete.
        /// </summary>
        /// <param name="continuationContext">The context on which context the next continuation will be executed. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the next continuation to be invoked asynchronously.</param>
        /// <param name="cancelationToken">If canceled before this is complete, the returned <see cref="Promise{T}"/> will be canceled, and the cancelation will propagate on the provided <paramref name="continuationContext"/>.</param>
        [MethodImpl(Internal.InlineOption)]
        public Promise WaitAsync(SynchronizationContext continuationContext, bool forceAsync = false, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            return Internal.PromiseRefBase.CallbackHelperVoid.WaitAsync(this, Internal.SynchronizationOption.Explicit, continuationContext, forceAsync, cancelationToken);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/> that inherits the state of this, or will be canceled if/when the <paramref name="cancelationToken"/> is canceled before this is complete.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise WaitAsync(CancelationToken cancelationToken)
        {
            ValidateOperation(1);
            return Internal.PromiseRefBase.CallbackHelperVoid.WaitAsync(this, cancelationToken);
        }

        /// <summary>
        /// Add a finally callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked.
        /// <para/>If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise"/> will be rejected with that exception,
        /// otherwise it will be resolved, rejected, or canceled with the same value or reason as this.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Finally(Action onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, nameof(onFinally), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddFinally(this, Internal.PromiseRefBase.DelegateWrapper.Create(onFinally));
        }

        /// <summary>
        /// Add a finally callback. Returns a new <see cref="Promise"/>.
        /// </summary>
        /// <remarks>
        /// When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked, and the new <see cref="Promise"/> will wait for the returned <see cref="Promise"/> to settle.
        /// <para/>
        /// If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise"/> will be rejected with that exception.
        /// <para/>
        /// If the <see cref="Promise"/> returned from <paramref name="onFinally"/> is rejected or canceled, the new <see cref="Promise"/> will adopt its state.
        /// <para/>
        /// Otherwise, the new <see cref="Promise"/> will adopt the state of this when the <see cref="Promise"/> returned from <paramref name="onFinally"/> is resolved.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public Promise Finally(Func<Promise> onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, nameof(onFinally), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddFinallyWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onFinally));
        }

        /// <summary>
        /// Add a cancel callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is canceled, <paramref name="onCanceled"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise CatchCancelation(Action onCanceled, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, nameof(onCanceled), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddCancel(this, Internal.PromiseRefBase.DelegateWrapper.Create(onCanceled), cancelationToken);
        }

        /// <summary>
        /// Add a cancel callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is canceled, <paramref name="onCanceled"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise CatchCancelation(Func<Promise> onCanceled, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, nameof(onCanceled), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddCancelWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onCanceled), cancelationToken);
        }

        #region Resolve Callbacks
        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then(Action onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolve(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolve(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<Promise> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), cancelationToken);
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch(Action onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TReject>(Action<TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch(Func<Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TReject>(Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Action onResolved, Action onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Action onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveReject(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Func<Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Action onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Action onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Func<Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>
                .AddResolveRejectWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onResolved), Internal.PromiseRefBase.DelegateWrapper.Create(onRejected), cancelationToken);
        }
        #endregion

        #region Continue Callbacks
        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith(Action<ResultContainer> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddContinue(this, Internal.PromiseRefBase.DelegateWrapper.Create(onContinue), cancelationToken);
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TResult>(Func<ResultContainer, TResult> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddContinue(this, Internal.PromiseRefBase.DelegateWrapper.Create(onContinue), cancelationToken);
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith(Func<ResultContainer, Promise> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddContinueWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onContinue), cancelationToken);
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TResult>(Func<ResultContainer, Promise<TResult>> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddContinueWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(onContinue), cancelationToken);
        }
        #endregion

        // Capture values below.

        /// <summary>
        /// Capture a value and add a finally callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked with <paramref name="finallyCaptureValue"/>.
        /// <para/>If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise"/> will be rejected with that exception,
        /// otherwise it will be resolved, rejected, or canceled with the same value or reason as this.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Finally<TCaptureFinally>(TCaptureFinally finallyCaptureValue, Action<TCaptureFinally> onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, nameof(onFinally), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddFinally(this, Internal.PromiseRefBase.DelegateWrapper.Create(finallyCaptureValue, onFinally));
        }

        /// <summary>
        /// Capture a value and add a finally callback. Returns a new <see cref="Promise"/>.
        /// </summary>
        /// <remarks>
        /// When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked with <paramref name="finallyCaptureValue"/>,
        /// and the new <see cref="Promise"/> will wait for the returned <see cref="Promise"/> to settle.
        /// <para/>
        /// If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise"/> will be rejected with that exception.
        /// <para/>
        /// If the <see cref="Promise"/> returned from <paramref name="onFinally"/> is rejected or canceled, the new <see cref="Promise"/> will adopt its state.
        /// <para/>
        /// Otherwise, the new <see cref="Promise"/> will adopt the state of this when the <see cref="Promise"/> returned from <paramref name="onFinally"/> is resolved.
        /// </remarks>
        [MethodImpl(Internal.InlineOption)]
        public Promise Finally<TCaptureFinally>(TCaptureFinally finallyCaptureValue, Func<TCaptureFinally, Promise> onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, nameof(onFinally), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddFinallyWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(finallyCaptureValue, onFinally));
        }

        /// <summary>
        /// Capture a value and add a cancel callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is canceled, <paramref name="onCanceled"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise CatchCancelation<TCaptureCancel>(TCaptureCancel cancelCaptureValue, Action<TCaptureCancel> onCanceled, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, nameof(onCanceled), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddCancel(this, Internal.PromiseRefBase.DelegateWrapper.Create(cancelCaptureValue, onCanceled), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a cancel callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is canceled, <paramref name="onCanceled"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise CatchCancelation<TCaptureCancel>(TCaptureCancel cancelCaptureValue, Func<TCaptureCancel, Promise> onCanceled, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, nameof(onCanceled), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddCancelWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(cancelCaptureValue, onCanceled), cancelationToken);
        }

        #region Resolve Callbacks
        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolve(this, Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolve(this, Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved), cancelationToken);
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Action onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Action onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Action onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveReject(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Action onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Action onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is assignable to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled, the new <see cref="Promise{T}"/> will be canceled.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, nameof(onResolved), 1);
            ValidateArgument(onRejected, nameof(onRejected), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddResolveRejectWait(this,
                Internal.PromiseRefBase.DelegateWrapper.Create(resolveCaptureValue, onResolved),
                Internal.PromiseRefBase.DelegateWrapper.Create(rejectCaptureValue, onRejected),
                cancelationToken);
        }
        #endregion

        #region Continue Callbacks
        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith<TCapture>(TCapture continueCaptureValue, Action<TCapture, ResultContainer> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddContinue(this, Internal.PromiseRefBase.DelegateWrapper.Create(continueCaptureValue, onContinue), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TCapture, TResult>(TCapture continueCaptureValue, Func<TCapture, ResultContainer, TResult> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddContinue(this, Internal.PromiseRefBase.DelegateWrapper.Create(continueCaptureValue, onContinue), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith<TCapture>(TCapture continueCaptureValue, Func<TCapture, ResultContainer, Promise> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperVoid.AddContinueWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(continueCaptureValue, onContinue), cancelationToken);
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TCapture, TResult>(TCapture continueCaptureValue, Func<TCapture, ResultContainer, Promise<TResult>> onContinue, CancelationToken cancelationToken = default)
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, nameof(onContinue), 1);

            return Internal.PromiseRefBase.CallbackHelperResult<TResult>.AddContinueWait(this, Internal.PromiseRefBase.DelegateWrapper.Create(continueCaptureValue, onContinue), cancelationToken);
        }
        #endregion

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="Promise"/>.</summary>
        [MethodImpl(Internal.InlineOption)]
        public bool Equals(Promise other)
            => this == other;

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
        public override bool Equals(object obj)
            => obj is Promise promise && Equals(promise);

        // Promises really shouldn't be used for lookups, but GetHashCode is overridden to complement ==.
        /// <summary>Returns the hash code for this instance.</summary>
        [MethodImpl(Internal.InlineOption)]
        public override int GetHashCode()
            => Internal.BuildHashCode(_ref, _id.GetHashCode(), 0);

        /// <summary>Returns a value indicating whether two <see cref="Promise"/> values are equal.</summary>
        [MethodImpl(Internal.InlineOption)]
        public static bool operator ==(Promise lhs, Promise rhs)
            => lhs._ref == rhs._ref
                & lhs._id == rhs._id;

        /// <summary>Returns a value indicating whether two <see cref="Promise"/> values are not equal.</summary>
        [MethodImpl(Internal.InlineOption)]
        public static bool operator !=(Promise lhs, Promise rhs)
            => !(lhs == rhs);

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            var _this = this;
            string state = _this._ref == null
                ? State.Resolved.ToString()
                : _this._ref.GetIsValid(_this._id)
                ? _this._ref.State.ToString()
                : "Invalid";
            return string.Format("Type: Promise, State: {0}", state);
        }
    }
}