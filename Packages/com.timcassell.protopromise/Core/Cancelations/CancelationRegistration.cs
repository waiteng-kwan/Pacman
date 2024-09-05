﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Proto.Promises
{
    /// <summary>
    /// Represents a callback delegate that has been registered with a <see cref="CancelationToken"/>.
    /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
    [DebuggerNonUserCode, StackTraceHidden]
#endif
    public readonly partial struct CancelationRegistration : IEquatable<CancelationRegistration>, IDisposable
    {
        private readonly Internal.CancelationRef _ref;
        private readonly Internal.CancelationCallbackNode _node;
        private readonly int _nodeId;
        private readonly int _tokenId;

        internal CancelationRegistration(Internal.CancelationRef cancelationRef, Internal.CancelationCallbackNode node, int nodeId, int tokenId)
        {
            _ref = cancelationRef;
            _node = node;
            _nodeId = nodeId;
            _tokenId = tokenId;
        }

        /// <summary>
        /// Get the <see cref="CancelationToken"/> associated with this <see cref="CancelationRegistration"/>.
        /// </summary>
        public CancelationToken Token
        {
            [MethodImpl(Internal.InlineOption)]
            get => new CancelationToken(_ref, _tokenId);
        }

        /// <summary>
        /// Get whether the callback is registered.
        /// </summary>
        public bool IsRegistered
            => Internal.CancelationCallbackNode.GetIsRegistered(_ref, _node, _nodeId, _tokenId);

        /// <summary>
        /// Get whether the callback is registered and whether the associated <see cref="CancelationToken"/> is requesting cancelation as an atomic operation.
        /// </summary>
        /// <param name="isRegistered">true if this is registered, false otherwise</param>
        /// <param name="isTokenCancelationRequested">true if the associated <see cref="CancelationToken"/> is requesting cancelation, false otherwise</param>
        public void GetIsRegisteredAndIsCancelationRequested(out bool isRegistered, out bool isTokenCancelationRequested)
            => isRegistered = Internal.CancelationCallbackNode.GetIsRegisteredAndIsCanceled(_ref, _node, _nodeId, _tokenId, out isTokenCancelationRequested);

        /// <summary>
        /// Try to unregister the callback from the associated <see cref="CancelationToken"/>. Returns true if the callback was successfully unregistered, false otherwise.
        /// </summary>
        /// <returns>true if the callback was previously registered and the associated <see cref="CancelationToken"/> not yet canceled and the associated <see cref="CancelationSource"/> not yet disposed, false otherwise</returns>
        public bool TryUnregister()
            => Internal.CancelationCallbackNode.TryUnregister(_ref, _node, _nodeId, _tokenId);

        /// <summary>
        /// Try to unregister the callback from the associated <see cref="CancelationToken"/>. Returns true if the callback was successfully unregistered, false otherwise.
        /// <paramref name="isTokenCancelationRequested"/> will be true if the associated <see cref="CancelationToken"/> is requesting cancelation, false otherwise.
        /// </summary>
        /// <param name="isTokenCancelationRequested">true if the associated <see cref="CancelationToken"/> is requesting cancelation, false otherwise</param>
        /// <returns>true if the callback was previously registered and the associated <see cref="CancelationSource"/> not yet canceled or disposed, false otherwise</returns>
        public bool TryUnregister(out bool isTokenCancelationRequested)
            => Internal.CancelationCallbackNode.TryUnregister(_ref, _node, _nodeId, _tokenId, out isTokenCancelationRequested);

        /// <summary>
        /// Try to unregister the callback from the associated <see cref="CancelationToken"/>.
        /// If the callback is currently executing, this method will wait until it completes,
        /// except in the degenerate case where the callback itself is unregistering itself.
        /// </summary>
        public void Dispose()
            => Internal.CancelationCallbackNode.TryUnregisterOrWaitForCallbackToComplete(_ref, _node, _nodeId, _tokenId);

        /// <summary>
        /// Try to unregister the callback from the associated <see cref="CancelationToken"/>.
        /// The returned <see cref="Promise"/> will be resolved once the associated callback
        /// is unregistered without having executed or once it's finished executing, except
        /// in the degenerate case where the callback itself is unregistering itself.
        /// </summary>
        public Promise DisposeAsync()
            => Internal.CancelationCallbackNode.TryUnregisterOrWaitForCallbackToCompleteAsync(_ref, _node, _nodeId, _tokenId);

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="CancelationRegistration"/>.</summary>
        public bool Equals(CancelationRegistration other)
            => this == other;

        /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
        public override bool Equals(object obj)
            => obj is CancelationRegistration registration && Equals(registration);

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode()
            => Internal.BuildHashCode(_node, _nodeId.GetHashCode(), _tokenId.GetHashCode());

        /// <summary>Returns a value indicating whether two <see cref="CancelationRegistration"/> values are equal.</summary>
        public static bool operator ==(CancelationRegistration lhs, CancelationRegistration rhs)
            => lhs._ref == rhs._ref
            & lhs._node == rhs._node
            & lhs._nodeId == rhs._nodeId
            & lhs._tokenId == rhs._tokenId;

        /// <summary>Returns a value indicating whether two <see cref="CancelationRegistration"/> values are not equal.</summary>
        public static bool operator !=(CancelationRegistration lhs, CancelationRegistration rhs)
            => !(lhs == rhs);
    }

#if UNITY_2021_2_OR_NEWER || !UNITY_2018_3_OR_NEWER
    partial struct CancelationRegistration : IAsyncDisposable
    {
        System.Threading.Tasks.ValueTask IAsyncDisposable.DisposeAsync()
            => DisposeAsync();
    }
#endif
}