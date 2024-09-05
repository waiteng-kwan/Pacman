﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Proto.Promises
{
    partial class Internal
    {
        partial class PromiseRefBase
        {
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class AllPromiseGroup<T> : MergePromiseGroupBase<IList<T>>
            {
                [MethodImpl(InlineOption)]
                private static AllPromiseGroup<T> GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<AllPromiseGroup<T>>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new AllPromiseGroup<T>()
                        : obj.UnsafeAs<AllPromiseGroup<T>>();
                }

                [MethodImpl(InlineOption)]
                internal static AllPromiseGroup<T> GetOrCreate(CancelationRef cancelationSource, IList<T> value)
                {
                    var promise = GetOrCreate();
                    promise._result = value;
                    promise._completeState = Promise.State.Resolved; // Default to Resolved state. If the promise is actually canceled or rejected, the state will be overwritten.
                    promise.Reset(cancelationSource);
                    return promise;
                }

                internal override void MaybeDispose()
                {
                    Dispose();
                    ObjectPool.MaybeRepool(this);
                }

                internal override void Handle(PromisePassThroughForMergeGroup passthrough, PromiseRefBase handler, Promise.State state)
                {
                    // We store the passthrough until all promises are complete,
                    // so that items won't be written to the list while it's being expanded on another thread.
                    RemovePromiseAndSetCompletionState(handler, state);
                    _completedPassThroughs.PushInterlocked(passthrough);
                    if (state != Promise.State.Resolved)
                    {
                        CancelGroup();
                    }
                    if (TryComplete())
                    {
                        // All promises are complete.
                        HandleNextInternal(CompleteAndGetState());
                    }
                }

                private Promise.State CompleteAndGetState()
                {
                    var state = _completeState;
                    var passthroughs = _completedPassThroughs.TakeAndClear();
                    while (passthroughs.IsNotEmpty)
                    {
                        var passthrough = passthroughs.Pop();
                        var owner = passthrough.Owner;
                        _result[passthrough.Index] = owner.GetResult<T>();
                        if (owner.State == Promise.State.Rejected)
                        {
                            RecordException(owner._rejectContainer.GetValueAsException());
                        }
                        passthrough.Dispose();
                    }

                    if (_exceptions != null)
                    {
                        state = Promise.State.Rejected;
                        _rejectContainer = CreateRejectContainer(new AggregateException(_exceptions), int.MinValue, null, this);
                        _exceptions = null;
                    }

                    return state;
                }

                internal void MarkReady(int totalPromises)
                {
                    // This method is called after all promises have been hooked up to this.
                    if (MarkReadyAndGetIsComplete(totalPromises))
                    {
                        // All promises already completed.
                        _next = PromiseCompletionSentinel.s_instance;
                        SetCompletionState(CompleteAndGetState());
                    }
                }
            }
        } // class PromiseRefBase

        [MethodImpl(InlineOption)]
        internal static PromiseRefBase.AllPromiseGroup<T> GetOrCreateAllPromiseGroup<T>(CancelationRef cancelationSource, IList<T> value)
            => PromiseRefBase.AllPromiseGroup<T>.GetOrCreate(cancelationSource, value);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidAllGroup(int skipFrames)
            => throw new InvalidOperationException("The promise all group is invalid.", GetFormattedStacktrace(skipFrames + 1));
    } // class Internal
}