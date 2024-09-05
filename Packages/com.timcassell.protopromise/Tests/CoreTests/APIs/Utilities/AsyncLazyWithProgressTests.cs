#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using NUnit.Framework;
using Proto.Promises;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoPromiseTests.APIs.Utilities
{
    public class AsyncLazyWithProgressTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.Setup();
        }

        [TearDown]
        public void Teardown()
        {
            TestHelper.Cleanup();
        }

        [Test]
        public void AsyncLazy_ProgressIsReportedProperly(
            [Values] ProgressType progressType,
            [Values] SynchronizationType synchronizationType)
        {
            var deferred = Promise.NewDeferred<int>();
            var progressToken = default(ProgressToken);
            var lazy = new AsyncLazy<int>(token =>
            {
                progressToken = token;
                return deferred.Promise;
            });

            ProgressHelper progressHelper = new ProgressHelper(progressType, synchronizationType);
            var progress = progressHelper.ToProgress();

            lazy.GetResultAsync(progress.Token)
                .Then(progress.DisposeAsync)
                .Forget();

            progressHelper.AssertCurrentProgress(double.NaN, false, false);

            progressHelper.ReportProgressAndAssertResult(progressToken, 0.5f, 0.5f);
            progressHelper.ResolveAndAssertResult(deferred, 1, 1f);
        }

        [Test]
        public void AsyncLazy_NeverAwaited_DoesNotCallFunc()
        {
            var lazy = new AsyncLazy<int>(progressToken => { throw new Exception(); });
            GC.KeepAlive(lazy);
        }

        [Test]
        public void AsyncLazy_GetResultAsync_CallsFunc()
        {
            bool wasCalled = false;
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                wasCalled = true;
                return Promise.Resolved(13);
            });

            lazy.GetResultAsync(ProgressToken.None).Forget();

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void AsyncLazy_CallsFuncDirectly_Then()
        {
            var testThread = Thread.CurrentThread;
            Thread funcThread = null;
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                funcThread = Thread.CurrentThread;
                return Promise.Resolved(13);
            });

            bool isComplete = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v =>
                {
                    isComplete = true;
                    Assert.AreEqual(13, v);
                })
                .Forget();

            Assert.IsTrue(isComplete);
            Assert.AreEqual(testThread, funcThread);
        }

        [Test]
        public void AsyncLazy_GetResultAsyncYieldsFuncValue_Then()
        {
            var deferred = Promise.NewDeferred();
            var lazy = new AsyncLazy<int>(progressToken =>
                deferred.Promise.Then(() => 13)
            );

            bool isComplete = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v =>
                {
                    isComplete = true;
                    Assert.AreEqual(13, v);
                })
                .Forget();

            deferred.Resolve();
            Assert.IsTrue(isComplete);
        }

        [Test]
        public void AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce_Then()
        {
            int invokeCount = 0;
            var deferred = Promise.NewDeferred();
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                Assert.AreEqual(1, Interlocked.Increment(ref invokeCount));
                return deferred.Promise.Then(() => 13);
            });

            var promise1 = Promise.Run(() => lazy.GetResultAsync(ProgressToken.None), SynchronizationOption.Synchronous);
            var promise2 = Promise.Run(() => lazy.GetResultAsync(ProgressToken.None), SynchronizationOption.Synchronous);

            bool isComplete = false;
            Promise.All(promise1, promise2)
                .Then(results =>
                {
                    isComplete = true;
                    CollectionAssert.AreEqual(new[] { 13, 13 }, results);
                })
                .Forget();

            deferred.Resolve();
            Assert.IsTrue(isComplete);
            Assert.AreEqual(1, invokeCount);
        }

        [Test]
        public void AsyncLazy_SingleAwait_FailureIsCleanedUpProperly_Then()
        {
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                throw new System.InvalidOperationException();
            });

            bool didThrow = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Catch((System.InvalidOperationException e) => didThrow = true)
                .Forget();

            Assert.IsTrue(didThrow);
        }

        [Test]
        public void AsyncLazy_RetriesAndDoesNotCacheImmediateFailure_Then()
        {
            int invokeCount = 0;
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                var count = Interlocked.Increment(ref invokeCount);
                if (count == 1)
                    throw new System.InvalidOperationException();
                return Promise.Resolved(13);
            });

            bool didThrow = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Catch((System.InvalidOperationException e) => didThrow = true)
                .Forget();

            Assert.IsTrue(didThrow);

            lazy.GetResultAsync(ProgressToken.None)
                .Then(v => Assert.AreEqual(13, v))
                .Forget();
            Assert.AreEqual(2, invokeCount);
        }

        [Test]
        public void AsyncLazy_RetriesAndDoesNotCacheFailure_Then()
        {
            int invokeCount = 0;
            var deferred = Promise.NewDeferred();
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                var count = Interlocked.Increment(ref invokeCount);
                return deferred.Promise.Then(() =>
                {
                    if (count == 1)
                        throw new System.InvalidOperationException();
                    return 13;
                });
            });

            bool didThrow = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Catch((System.InvalidOperationException e) => didThrow = true)
                .Forget();

            deferred.Resolve();
            Assert.IsTrue(didThrow);

            deferred = Promise.NewDeferred();
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v => Assert.AreEqual(13, v))
                .Forget();
            deferred.Resolve();
            Assert.AreEqual(2, invokeCount);
        }

        [Test]
        public void AsyncLazy_DoesNotRetryOnSuccess_Then()
        {
            int invokeCount = 0;
            var deferred = Promise.NewDeferred();
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                var count = Interlocked.Increment(ref invokeCount);
                return deferred.Promise.Then(() =>
                {
                    if (count == 1)
                        throw new System.InvalidOperationException();
                    return 13;
                });
            });

            bool didThrow = false;
            lazy.GetResultAsync(ProgressToken.None)
                .Catch((System.InvalidOperationException e) => didThrow = true)
                .Forget();

            deferred.Resolve();
            Assert.IsTrue(didThrow);

            deferred = Promise.NewDeferred();
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v => Assert.AreEqual(13, v))
                .Forget();
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v => Assert.AreEqual(13, v))
                .Forget();
            lazy.GetResultAsync(ProgressToken.None)
                .Then(v => Assert.AreEqual(13, v))
                .Forget();
            deferred.Resolve();
            Assert.AreEqual(2, invokeCount);
        }

#if !UNITY_WEBGL
        [Test]
        public void AsyncLazy_WithPromiseRun_CallsFuncOnBackgroundContext_Then()
        {
            var testThread = Thread.CurrentThread;
            Thread funcThread = null;
            var lazy = new AsyncLazy<int>(progressToken => Promise.Run(() =>
            {
                funcThread = Thread.CurrentThread;
                return Promise.Resolved(13);
            }, SynchronizationOption.Background));

            Assert.AreEqual(13, lazy.GetResultAsync(ProgressToken.None).WaitWithTimeout(TimeSpan.FromSeconds(1)));

            Assert.AreNotEqual(testThread, funcThread);
        }
#endif // !UNITY_WEBGL

        [Test]
        public void AsyncLazy_CallsFuncDirectly_Async()
        {
            AsyncLazy_CallsFuncDirectly_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_CallsFuncDirectly_Core()
        {
            var testThread = Thread.CurrentThread;
            Thread funcThread = null;
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                funcThread = Thread.CurrentThread;
                return Promise.Resolved(13);
            });

            await lazy;

            Assert.AreEqual(testThread, funcThread);
        }

        [Test]
        public void AsyncLazy_Await_ReturnsFuncValue_Async()
        {
            AsyncLazy_Await_ReturnsFuncValue_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_Await_ReturnsFuncValue_Core()
        {
            var lazy = new AsyncLazy<int>(async progressToken =>
            {
                await Task.Yield();
                return 13;
            });

            var result = await lazy;
            Assert.AreEqual(13, result);
        }

        [Test]
        public void AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce_Async()
        {
            AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce_Core()
        {
            int invokeCount = 0;
            var deferred = Promise.NewDeferred();
            var lazy = new AsyncLazy<int>(async progressToken =>
            {
                Assert.AreEqual(1, Interlocked.Increment(ref invokeCount));
                await deferred.Promise;
                return 13;
            });

            var promise1 = Promise.Run(async () => await lazy, SynchronizationOption.Synchronous);
            var promise2 = Promise.Run(async () => await lazy, SynchronizationOption.Synchronous);

            deferred.Resolve();
            var results = await Promise.All(promise1, promise2);
            CollectionAssert.AreEqual(new[] { 13, 13 }, results);
            Assert.AreEqual(1, invokeCount);
        }

        [Test]
        public void AsyncLazy_SingleAwait_FailureIsCleanedUpProperly_Async()
        {
            AsyncLazy_SingleAwait_FailureIsCleanedUpProperly_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_SingleAwait_FailureIsCleanedUpProperly_Core()
        {
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                throw new System.InvalidOperationException();
            });

            bool didThrow = false;
            try
            {
                await lazy;
            }
            catch (System.InvalidOperationException)
            {
                didThrow = true;
            }

            Assert.IsTrue(didThrow);
        }

        [Test]
        public void AsyncLazy_RetriesAndDoesNotCacheImmediateFailure_Async()
        {
            AsyncLazy_RetriesAndDoesNotCacheImmediateFailure_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_RetriesAndDoesNotCacheImmediateFailure_Core()
        {
            int invokeCount = 0;
            var lazy = new AsyncLazy<int>(progressToken =>
            {
                var count = Interlocked.Increment(ref invokeCount);
                if (count == 1)
                    throw new System.InvalidOperationException();
                return Promise.Resolved(13);
            });

            bool didThrow = false;
            try
            {
                await lazy;
            }
            catch (System.InvalidOperationException)
            {
                didThrow = true;
            }

            Assert.IsTrue(didThrow);
            Assert.AreEqual(13, await lazy);
            Assert.AreEqual(2, invokeCount);
        }

        [Test]
        public void AsyncLazy_RetriesAndDoesNotCacheFailure_Async()
        {
            AsyncLazy_RetriesAndDoesNotCacheFailure_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_RetriesAndDoesNotCacheFailure_Core()
        {
            int invokeCount = 0;
            var lazy = new AsyncLazy<int>(async progressToken =>
            {
                var count = Interlocked.Increment(ref invokeCount);
                await Task.Yield();
                if (count == 1)
                    throw new System.InvalidOperationException();
                return 13;
            });

            bool didThrow = false;
            try
            {
                await lazy;
            }
            catch (System.InvalidOperationException)
            {
                didThrow = true;
            }

            Assert.IsTrue(didThrow);
            Assert.AreEqual(13, await lazy);
            Assert.AreEqual(2, invokeCount);
        }

        [Test]
        public void AsyncLazy_DoesNotRetryOnSuccess_Async()
        {
            AsyncLazy_DoesNotRetryOnSuccess_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_DoesNotRetryOnSuccess_Core()
        {
            int invokeCount = 0;
            var lazy = new AsyncLazy<int>(async progressToken =>
            {
                Interlocked.Increment(ref invokeCount);
                await Task.Yield();
                if (invokeCount == 1)
                    throw new System.InvalidOperationException();
                return 13;
            });

            bool didThrow = false;
            try
            {
                await lazy;
            }
            catch (System.InvalidOperationException)
            {
                didThrow = true;
            }

            Assert.IsTrue(didThrow);

            await lazy;
            await lazy;

            Assert.AreEqual(13, await lazy);
            Assert.AreEqual(2, invokeCount);
        }

#if !UNITY_WEBGL
        [Test]
        public void AsyncLazy_SwitchToBackground_CallsFuncOnBackgroundContext_Async()
        {
            AsyncLazy_SwitchToBackground_CallsFuncOnBackgroundContext_Core()
                .WaitWithTimeoutWhileExecutingForegroundContext(TimeSpan.FromSeconds(1));
        }

        private async Promise AsyncLazy_SwitchToBackground_CallsFuncOnBackgroundContext_Core()
        {
            var testThread = Thread.CurrentThread;
            Thread funcThread = null;
            var lazy = new AsyncLazy<int>(async progressToken =>
            {
                await Promise.SwitchToBackground(true);
                funcThread = Thread.CurrentThread;
                return 13;
            });

            await lazy;

            Assert.AreNotEqual(testThread, funcThread);
        }
#endif // !UNITY_WEBGL
    }
}