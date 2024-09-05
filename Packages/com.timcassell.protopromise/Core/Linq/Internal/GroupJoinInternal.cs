#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using Proto.Promises.CompilerServices;
using Proto.Promises.Collections;
using Proto.Promises.Linq;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Proto.Promises
{
    partial class Internal
    {
#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal static class GroupJoinHelper<TKey>
        {
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private readonly struct GroupJoinSyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer> : IAsyncIterator<(TOuter Outer, TempCollection<TInner> InnerElements)>
                where TOuterKeySelector : IFunc<TOuter, TKey>
                where TInnerKeySelector : IFunc<TInner, TKey>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                private readonly AsyncEnumerator<TOuter> _outerAsyncEnumerator;
                private readonly AsyncEnumerator<TInner> _innerAsyncEnumerator;
                private readonly TOuterKeySelector _outerKeySelector;
                private readonly TInnerKeySelector _innerKeySelector;
                private readonly TEqualityComparer _comparer;

                internal GroupJoinSyncIterator(AsyncEnumerator<TOuter> outerAsyncEnumerator,
                    AsyncEnumerator<TInner> innerAsyncEnumerator,
                    TOuterKeySelector outerKeySelector,
                    TInnerKeySelector innerKeySelector,
                    TEqualityComparer comparer)
                {
                    _outerAsyncEnumerator = outerAsyncEnumerator;
                    _innerAsyncEnumerator = innerAsyncEnumerator;
                    _outerKeySelector = outerKeySelector;
                    _innerKeySelector = innerKeySelector;
                    _comparer = comparer;
                }

                public async AsyncIteratorMethod Start(AsyncStreamWriter<(TOuter Outer, TempCollection<TInner> InnerElements)> writer, CancelationToken cancelationToken)
                {
                    // The enumerators were retrieved without a cancelation token when the original function was called.
                    // We need to propagate the token that was passed in, so we assign it before starting iteration.
                    _outerAsyncEnumerator._target._cancelationToken = cancelationToken;
                    _innerAsyncEnumerator._target._cancelationToken = cancelationToken;

                    // We could do await Lookup<TKey, TInner>.GetOrCreateAsync(...), but it's more efficient to do it manually so we won't allocate the Lookup class and a separate async state machine.
                    LookupImpl<TKey, TInner, TEqualityComparer> lookup = new LookupImpl<TKey, TInner, TEqualityComparer>(_comparer, true);
                    var emptyInnerElements = new TempCollectionBuilder<TInner>(0);
                    try
                    {
                        while (await _innerAsyncEnumerator.MoveNextAsync())
                        {
                            var item = _innerAsyncEnumerator.Current;
                            var key = _innerKeySelector.Invoke(item);
                            // Unlike GroupBy, GroupJoin ignores null keys.
                            if (key != null)
                            {
                                lookup.GetOrCreateGrouping(key, true).Add(item);
                            }
                        }
                        // We don't dispose the enumerators until the owner is disposed.
                        // This is in case either enumerator contains TempCollection that they will still be valid until the owner is disposed.

                        while (await _outerAsyncEnumerator.MoveNextAsync())
                        {
                            var outer = _outerAsyncEnumerator.Current;
                            var g = lookup.GetGrouping(_outerKeySelector.Invoke(outer));
                            if (g == null)
                            {
                                await writer.YieldAsync((outer, emptyInnerElements.View));
                            }
                            else
                            {
                                await writer.YieldAsync((outer, g._elements.View));
                            }
                        }

                        // We yield and wait for the enumerator to be disposed, but only if there were no exceptions.
                        await writer.YieldAsync(default).ForLinqExtension();
                    }
                    finally
                    {
                        emptyInnerElements.Dispose();
                        lookup.Dispose();
                        try
                        {
                            await _innerAsyncEnumerator.DisposeAsync();
                        }
                        finally
                        {
                            await _outerAsyncEnumerator.DisposeAsync();
                        }
                    }
                }

                public Promise DisposeAsyncWithoutStart()
                {
                    // We consume less memory by using .Finally instead of async/await.
                    return _innerAsyncEnumerator.DisposeAsync()
                        .Finally(_outerAsyncEnumerator, e => e.DisposeAsync());
                }
            }

            internal static AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)> GroupJoin<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(
                AsyncEnumerator<TOuter> outerAsyncEnumerator,
                AsyncEnumerator<TInner> innerAsyncEnumerator,
                TOuterKeySelector outerKeySelector,
                TInnerKeySelector innerKeySelector,
                TEqualityComparer comparer)
                where TOuterKeySelector : IFunc<TOuter, TKey>
                where TInnerKeySelector : IFunc<TInner, TKey>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                return AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)>.Create(
                    new GroupJoinSyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(outerAsyncEnumerator, innerAsyncEnumerator, outerKeySelector, innerKeySelector, comparer));
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private readonly struct GroupJoinAsyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer> : IAsyncIterator<(TOuter Outer, TempCollection<TInner> InnerElements)>
                where TOuterKeySelector : IFunc<TOuter, Promise<TKey>>
                where TInnerKeySelector : IFunc<TInner, Promise<TKey>>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                private readonly AsyncEnumerator<TOuter> _outerAsyncEnumerator;
                private readonly AsyncEnumerator<TInner> _innerAsyncEnumerator;
                private readonly TOuterKeySelector _outerKeySelector;
                private readonly TInnerKeySelector _innerKeySelector;
                private readonly TEqualityComparer _comparer;

                internal GroupJoinAsyncIterator(AsyncEnumerator<TOuter> outerAsyncEnumerator,
                    AsyncEnumerator<TInner> innerAsyncEnumerator,
                    TOuterKeySelector outerKeySelector,
                    TInnerKeySelector innerKeySelector,
                    TEqualityComparer comparer)
                {
                    _outerAsyncEnumerator = outerAsyncEnumerator;
                    _innerAsyncEnumerator = innerAsyncEnumerator;
                    _outerKeySelector = outerKeySelector;
                    _innerKeySelector = innerKeySelector;
                    _comparer = comparer;
                }

                public async AsyncIteratorMethod Start(AsyncStreamWriter<(TOuter Outer, TempCollection<TInner> InnerElements)> writer, CancelationToken cancelationToken)
                {
                    // The enumerators were retrieved without a cancelation token when the original function was called.
                    // We need to propagate the token that was passed in, so we assign it before starting iteration.
                    _outerAsyncEnumerator._target._cancelationToken = cancelationToken;
                    _innerAsyncEnumerator._target._cancelationToken = cancelationToken;

                    // We could do await Lookup<TKey, TInner>.GetOrCreateAsync(...), but it's more efficient to do it manually so we won't allocate the Lookup class and a separate async state machine.
                    LookupImpl<TKey, TInner, TEqualityComparer> lookup = new LookupImpl<TKey, TInner, TEqualityComparer>(_comparer, true);
                    var emptyInnerElements = new TempCollectionBuilder<TInner>(0);
                    try
                    {
                        while (await _innerAsyncEnumerator.MoveNextAsync())
                        {
                            var item = _innerAsyncEnumerator.Current;
                            var key = await _innerKeySelector.Invoke(item);
                            // Unlike GroupBy, GroupJoin ignores null keys.
                            if (key != null)
                            {
                                lookup.GetOrCreateGrouping(key, true).Add(item);
                            }
                        }
                        // We don't dispose the enumerators until the owner is disposed.
                        // This is in case either enumerator contains TempCollection that they will still be valid until the owner is disposed.

                        while (await _outerAsyncEnumerator.MoveNextAsync())
                        {
                            var outer = _outerAsyncEnumerator.Current;
                            var g = lookup.GetGrouping(await _outerKeySelector.Invoke(outer));
                            if (g == null)
                            {
                                await writer.YieldAsync((outer, emptyInnerElements.View));
                            }
                            else
                            {
                                await writer.YieldAsync((outer, g._elements.View));
                            }
                        }

                        // We yield and wait for the enumerator to be disposed, but only if there were no exceptions.
                        await writer.YieldAsync(default).ForLinqExtension();
                    }
                    finally
                    {
                        emptyInnerElements.Dispose();
                        lookup.Dispose();
                        try
                        {
                            await _innerAsyncEnumerator.DisposeAsync();
                        }
                        finally
                        {
                            await _outerAsyncEnumerator.DisposeAsync();
                        }
                    }
                }

                public Promise DisposeAsyncWithoutStart()
                {
                    // We consume less memory by using .Finally instead of async/await.
                    return _innerAsyncEnumerator.DisposeAsync()
                        .Finally(_outerAsyncEnumerator, e => e.DisposeAsync());
                }
            }

            internal static AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)> GroupJoinAwait<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(
                AsyncEnumerator<TOuter> outerAsyncEnumerator,
                AsyncEnumerator<TInner> innerAsyncEnumerator,
                TOuterKeySelector outerKeySelector,
                TInnerKeySelector innerKeySelector,
                TEqualityComparer comparer)
                where TOuterKeySelector : IFunc<TOuter, Promise<TKey>>
                where TInnerKeySelector : IFunc<TInner, Promise<TKey>>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                return AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)>.Create(
                    new GroupJoinAsyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(outerAsyncEnumerator, innerAsyncEnumerator, outerKeySelector, innerKeySelector, comparer));
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private readonly struct ConfiguredGroupJoinSyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer> : IAsyncIterator<(TOuter Outer, TempCollection<TInner> InnerElements)>
                where TOuterKeySelector : IFunc<TOuter, TKey>
                where TInnerKeySelector : IFunc<TInner, TKey>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                private readonly ConfiguredAsyncEnumerable<TOuter>.Enumerator _configuredOuterAsyncEnumerator;
                private readonly AsyncEnumerator<TInner> _innerAsyncEnumerator;
                private readonly TOuterKeySelector _outerKeySelector;
                private readonly TInnerKeySelector _innerKeySelector;
                private readonly TEqualityComparer _comparer;

                internal ConfiguredGroupJoinSyncIterator(ConfiguredAsyncEnumerable<TOuter>.Enumerator configuredOuterAsyncEnumerator,
                    AsyncEnumerator<TInner> innerAsyncEnumerator,
                    TOuterKeySelector outerKeySelector,
                    TInnerKeySelector innerKeySelector,
                    TEqualityComparer comparer)
                {
                    _configuredOuterAsyncEnumerator = configuredOuterAsyncEnumerator;
                    _innerAsyncEnumerator = innerAsyncEnumerator;
                    _outerKeySelector = outerKeySelector;
                    _innerKeySelector = innerKeySelector;
                    _comparer = comparer;
                }

                public async AsyncIteratorMethod Start(AsyncStreamWriter<(TOuter Outer, TempCollection<TInner> InnerElements)> writer, CancelationToken cancelationToken)
                {
                    // The enumerator may have been configured with a cancelation token. We need to join the passed in token before starting iteration.
                    var enumerableRef = _configuredOuterAsyncEnumerator._enumerator._target;
                    var joinedCancelationSource = MaybeJoinCancelationTokens(enumerableRef._cancelationToken, cancelationToken, out enumerableRef._cancelationToken);
                    // Use the same cancelation token for both enumerators.
                    _innerAsyncEnumerator._target._cancelationToken = enumerableRef._cancelationToken;

                    // We could do await Lookup<TKey, TInner>.GetOrCreateAsync(...), but it's more efficient to do it manually so we won't allocate the Lookup class and a separate async state machine.
                    LookupImpl<TKey, TInner, TEqualityComparer> lookup = new LookupImpl<TKey, TInner, TEqualityComparer>(_comparer, true);
                    var emptyInnerElements = new TempCollectionBuilder<TInner>(0);
                    try
                    {
                        while (await _innerAsyncEnumerator.MoveNextAsync())
                        {
                            // The inner enumerator is not configured, so we have to switch to the context of the configured outer enumerator before invoking the inner key selector.
                            await _configuredOuterAsyncEnumerator.SwitchToContext();
                            var item = _innerAsyncEnumerator.Current;
                            var key = _innerKeySelector.Invoke(item);
                            // Unlike GroupBy, GroupJoin ignores null keys.
                            if (key != null)
                            {
                                lookup.GetOrCreateGrouping(key, true).Add(item);
                            }
                        }
                        // We don't dispose the enumerators until the owner is disposed.
                        // This is in case either enumerator contains TempCollection that they will still be valid until the owner is disposed.

                        while (await _configuredOuterAsyncEnumerator.MoveNextAsync())
                        {
                            var outer = _configuredOuterAsyncEnumerator.Current;
                            var g = lookup.GetGrouping(_outerKeySelector.Invoke(outer));
                            if (g == null)
                            {
                                await writer.YieldAsync((outer, emptyInnerElements.View));
                            }
                            else
                            {
                                await writer.YieldAsync((outer, g._elements.View));
                            }
                        }

                        // We yield and wait for the enumerator to be disposed, but only if there were no exceptions.
                        await writer.YieldAsync(default).ForLinqExtension();
                    }
                    finally
                    {
                        joinedCancelationSource.TryDispose();
                        emptyInnerElements.Dispose();
                        lookup.Dispose();
                        try
                        {
                            await _innerAsyncEnumerator.DisposeAsync();
                        }
                        finally
                        {
                            await _configuredOuterAsyncEnumerator.DisposeAsync();
                        }
                    }
                }

                public Promise DisposeAsyncWithoutStart()
                {
                    // We consume less memory by using .Finally instead of async/await.
                    return _innerAsyncEnumerator.DisposeAsync()
                        .Finally(_configuredOuterAsyncEnumerator, e => e.DisposeAsync());
                }
            }

            internal static AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)> GroupJoin<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(
                ConfiguredAsyncEnumerable<TOuter>.Enumerator configuredOuterAsyncEnumerator,
                AsyncEnumerator<TInner> innerAsyncEnumerator,
                TOuterKeySelector outerKeySelector,
                TInnerKeySelector innerKeySelector,
                TEqualityComparer comparer)
                where TOuterKeySelector : IFunc<TOuter, TKey>
                where TInnerKeySelector : IFunc<TInner, TKey>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                return AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)>.Create(
                    new ConfiguredGroupJoinSyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(configuredOuterAsyncEnumerator, innerAsyncEnumerator, outerKeySelector, innerKeySelector, comparer));
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private readonly struct ConfiguredGroupJoinAsyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer> : IAsyncIterator<(TOuter Outer, TempCollection<TInner> InnerElements)>
                where TOuterKeySelector : IFunc<TOuter, Promise<TKey>>
                where TInnerKeySelector : IFunc<TInner, Promise<TKey>>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                private readonly ConfiguredAsyncEnumerable<TOuter>.Enumerator _configuredOuterAsyncEnumerator;
                private readonly AsyncEnumerator<TInner> _innerAsyncEnumerator;
                private readonly TOuterKeySelector _outerKeySelector;
                private readonly TInnerKeySelector _innerKeySelector;
                private readonly TEqualityComparer _comparer;

                internal ConfiguredGroupJoinAsyncIterator(ConfiguredAsyncEnumerable<TOuter>.Enumerator configuredOuterAsyncEnumerator,
                    AsyncEnumerator<TInner> innerAsyncEnumerator,
                    TOuterKeySelector outerKeySelector,
                    TInnerKeySelector innerKeySelector,
                    TEqualityComparer comparer)
                {
                    _configuredOuterAsyncEnumerator = configuredOuterAsyncEnumerator;
                    _innerAsyncEnumerator = innerAsyncEnumerator;
                    _outerKeySelector = outerKeySelector;
                    _innerKeySelector = innerKeySelector;
                    _comparer = comparer;
                }

                public async AsyncIteratorMethod Start(AsyncStreamWriter<(TOuter Outer, TempCollection<TInner> InnerElements)> writer, CancelationToken cancelationToken)
                {
                    // The enumerator may have been configured with a cancelation token. We need to join the passed in token before starting iteration.
                    var enumerableRef = _configuredOuterAsyncEnumerator._enumerator._target;
                    var joinedCancelationSource = MaybeJoinCancelationTokens(enumerableRef._cancelationToken, cancelationToken, out enumerableRef._cancelationToken);
                    // Use the same cancelation token for both enumerators.
                    _innerAsyncEnumerator._target._cancelationToken = enumerableRef._cancelationToken;

                    // We could do await Lookup<TKey, TInner>.GetOrCreateAsync(...), but it's more efficient to do it manually so we won't allocate the Lookup class and a separate async state machine.
                    LookupImpl<TKey, TInner, TEqualityComparer> lookup = new LookupImpl<TKey, TInner, TEqualityComparer>(_comparer, true);
                    var emptyInnerElements = new TempCollectionBuilder<TInner>(0);
                    try
                    {
                        while (await _innerAsyncEnumerator.MoveNextAsync())
                        {
                            // The inner enumerator is not configured, so we have to switch to the context of the configured outer enumerator before invoking the inner key selector.
                            await _configuredOuterAsyncEnumerator.SwitchToContext();
                            var item = _innerAsyncEnumerator.Current;
                            var key = await _innerKeySelector.Invoke(item);
                            // Unlike GroupBy, GroupJoin ignores null keys.
                            if (key != null)
                            {
                                lookup.GetOrCreateGrouping(key, true).Add(item);
                            }
                        }
                        // We don't dispose the enumerators until the owner is disposed.
                        // This is in case either enumerator contains TempCollection that they will still be valid until the owner is disposed.

                        while (await _configuredOuterAsyncEnumerator.MoveNextAsync())
                        {
                            var outer = _configuredOuterAsyncEnumerator.Current;
                            var g = lookup.GetGrouping(await _outerKeySelector.Invoke(outer));
                            if (g == null)
                            {
                                await writer.YieldAsync((outer, emptyInnerElements.View));
                            }
                            else
                            {
                                await writer.YieldAsync((outer, g._elements.View));
                            }
                        }

                        // We yield and wait for the enumerator to be disposed, but only if there were no exceptions.
                        await writer.YieldAsync(default).ForLinqExtension();
                    }
                    finally
                    {
                        joinedCancelationSource.TryDispose();
                        emptyInnerElements.Dispose();
                        lookup.Dispose();
                        try
                        {
                            await _innerAsyncEnumerator.DisposeAsync();
                        }
                        finally
                        {
                            await _configuredOuterAsyncEnumerator.DisposeAsync();
                        }
                    }
                }

                public Promise DisposeAsyncWithoutStart()
                {
                    // We consume less memory by using .Finally instead of async/await.
                    return _innerAsyncEnumerator.DisposeAsync()
                        .Finally(_configuredOuterAsyncEnumerator, e => e.DisposeAsync());
                }
            }

            internal static AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)> GroupJoinAwait<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(
                ConfiguredAsyncEnumerable<TOuter>.Enumerator configuredOuterAsyncEnumerator,
                AsyncEnumerator<TInner> innerAsyncEnumerator,
                TOuterKeySelector outerKeySelector,
                TInnerKeySelector innerKeySelector,
                TEqualityComparer comparer)
                where TOuterKeySelector : IFunc<TOuter, Promise<TKey>>
                where TInnerKeySelector : IFunc<TInner, Promise<TKey>>
                where TEqualityComparer : IEqualityComparer<TKey>
            {
                return AsyncEnumerable<(TOuter Outer, TempCollection<TInner> InnerElements)>.Create(
                    new ConfiguredGroupJoinAsyncIterator<TOuter, TInner, TOuterKeySelector, TInnerKeySelector, TEqualityComparer>(configuredOuterAsyncEnumerator, innerAsyncEnumerator, outerKeySelector, innerKeySelector, comparer));
            }
        } // class Lookup<TKey, TElement>
    } // class Internal
} // namespace Proto.Promises