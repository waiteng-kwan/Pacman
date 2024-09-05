﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using Proto.Promises;
using NUnit.Framework;
using System.Threading;
using ProtoPromiseTests.Concurrency;
using System.Collections.Generic;
using System;

namespace ProtoPromiseTests.APIs
{
    public class NewAndRunTests
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

        private static IEnumerable<TestCaseData> GetArgs_New()
        {
            SynchronizationType[] synchronizationTypes = new SynchronizationType[]
            {
                SynchronizationType.Synchronous,
                //SynchronizationType.Foreground, // Ignore foreground to reduce number of tests, testing explicit is effectively the same.
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
                SynchronizationType.Explicit
            };

            SynchronizationType[] invokeContexts = new SynchronizationType[]
            {
                SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
            };

            CompleteType[] completeTypes = new CompleteType[]
            {
                CompleteType.Resolve,
                CompleteType.Reject,
                CompleteType.Cancel
            };
            
            bool[] throwInActions = new bool[] { true, false };

            foreach (SynchronizationType synchronizationType in synchronizationTypes)
            foreach (SynchronizationType invokeContext in invokeContexts)
            foreach (CompleteType completeType in completeTypes)
            foreach (bool throwInAction in throwInActions)
            {
                if (throwInAction && completeType == CompleteType.Resolve)
                {
                    continue;
                }
                yield return new TestCaseData(synchronizationType, invokeContext, completeType, throwInAction);
            }
        }

        [Test, TestCaseSource(nameof(GetArgs_New))]
        public void PromiseNewIsInvokedAndCompletedProperly_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";

            var deferred = default(Promise.Deferred);
            System.Action<Promise.Deferred> action = d =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                deferred = d;
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.New(action, TestHelper._foregroundContext)
                    : Promise.New(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            TestHelper.GetTryCompleterVoid(completeType, "Reject").Invoke(deferred);

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_New))]
        public void PromiseNewIsInvokedAndCompletedProperly_capture_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int captureValue = 10;

            var deferred = default(Promise.Deferred);
            System.Action<int, Promise.Deferred> action = (cv, d) =>
            {
                Assert.AreEqual(captureValue, cv);
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                deferred = d;
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.New(captureValue, action, TestHelper._foregroundContext)
                    : Promise.New(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            TestHelper.GetTryCompleterVoid(completeType, "Reject").Invoke(deferred);

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_New))]
        public void PromiseNewIsInvokedAndCompletedProperly_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;

            var deferred = default(Promise<int>.Deferred);
            System.Action<Promise<int>.Deferred> action = d =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                deferred = d;
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise<int>.New(action, TestHelper._foregroundContext)
                    : Promise<int>.New(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            TestHelper.GetTryCompleterT(completeType, expectedResolveValue, "Reject").Invoke(deferred);

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_New))]
        public void PromiseNewIsInvokedAndCompletedProperly_capture_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;
            int captureValue = 10;

            var deferred = default(Promise<int>.Deferred);
            System.Action<int, Promise<int>.Deferred> action = (cv, d) =>
            {
                Assert.AreEqual(captureValue, cv);
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                deferred = d;
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise<int>.New(captureValue, action, TestHelper._foregroundContext)
                    : Promise<int>.New(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            TestHelper.GetTryCompleterT(completeType, expectedResolveValue, "Reject").Invoke(deferred);

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }


        private static IEnumerable<TestCaseData> GetArgs_Run()
        {
            SynchronizationType[] synchronizationTypes = new SynchronizationType[]
            {
                SynchronizationType.Synchronous,
                //SynchronizationType.Foreground, // Ignore foreground to reduce number of tests, testing explicit is effectively the same.
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
                SynchronizationType.Explicit
            };

            SynchronizationType[] invokeContexts = new SynchronizationType[]
            {
                SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
            };

            CompleteType[] completeTypes = new CompleteType[]
            {
                CompleteType.Resolve,
                CompleteType.Reject,
                CompleteType.Cancel
            };

            foreach (SynchronizationType synchronizationType in synchronizationTypes)
            foreach (SynchronizationType invokeContext in invokeContexts)
            foreach (CompleteType completeType in completeTypes)
            {
                yield return new TestCaseData(synchronizationType, invokeContext, completeType);
            }
        }

        [Test, TestCaseSource(nameof(GetArgs_Run))]
        public void PromiseRunIsInvokedAndCompletedProperly_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";

            System.Action action = () =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (completeType == CompleteType.Reject)
                {
                    throw Promise.RejectException(expectedRejectValue);
                }
                else if (completeType == CompleteType.Cancel)
                {
                    throw Promise.CancelException();
                }
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(action, TestHelper._foregroundContext)
                    : Promise.Run(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_Run))]
        public void PromiseRunIsInvokedAndCompletedProperly_capture_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int captureValue = 10;

            System.Action<int> action = cv =>
            {
                Assert.AreEqual(captureValue, cv);
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (completeType == CompleteType.Reject)
                {
                    throw Promise.RejectException(expectedRejectValue);
                }
                else if (completeType == CompleteType.Cancel)
                {
                    throw Promise.CancelException();
                }
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(captureValue, action, TestHelper._foregroundContext)
                    : Promise.Run(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_Run))]
        public void PromiseRunIsInvokedAndCompletedProperly_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;

            System.Func<int> action = () =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (completeType == CompleteType.Reject)
                {
                    throw Promise.RejectException(expectedRejectValue);
                }
                else if (completeType == CompleteType.Cancel)
                {
                    throw Promise.CancelException();
                }
                return expectedResolveValue;
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(action, TestHelper._foregroundContext)
                    : Promise.Run(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_Run))]
        public void PromiseRunIsInvokedAndCompletedProperly_capture_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;
            int captureValue = 10;

            System.Func<int, int> action = cv =>
            {
                Assert.AreEqual(captureValue, cv);
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (completeType == CompleteType.Reject)
                {
                    throw Promise.RejectException(expectedRejectValue);
                }
                else if (completeType == CompleteType.Cancel)
                {
                    throw Promise.CancelException();
                }
                return expectedResolveValue;
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(captureValue, action, TestHelper._foregroundContext)
                    : Promise.Run(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }


        private static IEnumerable<TestCaseData> GetArgs_RunAdopt()
        {
            SynchronizationType[] synchronizationTypes = new SynchronizationType[]
            {
                SynchronizationType.Synchronous,
                SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
                SynchronizationType.Explicit
            };

            SynchronizationType[] invokeContexts = new SynchronizationType[]
            {
                SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
            };

            CompleteType[] completeTypes = new CompleteType[]
            {
                CompleteType.Resolve,
                CompleteType.Reject,
                CompleteType.Cancel
            };

            bool[] bools = new bool[] { true, false };

            foreach (SynchronizationType synchronizationType in synchronizationTypes)
            foreach (SynchronizationType invokeContext in invokeContexts)
            foreach (CompleteType completeType in completeTypes)
            foreach (bool throwInAction in bools)
            foreach (bool isAlreadyComplete in bools)
            {
                if (throwInAction && completeType == CompleteType.Resolve)
                {
                    continue;
                }
                if (throwInAction && isAlreadyComplete)
                {
                    continue;
                }
                yield return new TestCaseData(synchronizationType, invokeContext, completeType, throwInAction, isAlreadyComplete);
            }
        }

        [Test, TestCaseSource(nameof(GetArgs_RunAdopt))]
        public void PromiseRunIsInvokedAndCompletedProperly_adopt_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction,
            bool isAlreadyComplete)
        {
            bool isPending = !throwInAction & !isAlreadyComplete;
            
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";

            Action tryCompleter = () => { };

            System.Func<Promise> action = () =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                return TestHelper.BuildPromise(completeType, isAlreadyComplete, expectedRejectValue, out tryCompleter);
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(action, TestHelper._foregroundContext)
                    : Promise.Run(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            tryCompleter();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_RunAdopt))]
        public void PromiseRunIsInvokedAndCompletedProperly_adopt_capture_void(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction,
            bool isAlreadyComplete)
        {
            bool isPending = !throwInAction & !isAlreadyComplete;

            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int captureValue = 10;

            Action tryCompleter = () => { };

            System.Func<int, Promise> action = cv =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                Assert.AreEqual(captureValue, cv);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                return TestHelper.BuildPromise(completeType, isAlreadyComplete, expectedRejectValue, out tryCompleter);
            };

            var promise = default(Promise);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(captureValue, action, TestHelper._foregroundContext)
                    : Promise.Run(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            tryCompleter();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_RunAdopt))]
        public void PromiseRunIsInvokedAndCompletedProperly_adopt_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction,
            bool isAlreadyComplete)
        {
            bool isPending = !throwInAction & !isAlreadyComplete;

            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;

            Action tryCompleter = () => { };

            System.Func<Promise<int>> action = () =>
            {
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                return TestHelper.BuildPromise(completeType, isAlreadyComplete, expectedResolveValue, expectedRejectValue, out tryCompleter);
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(action, TestHelper._foregroundContext)
                    : Promise.Run(action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            tryCompleter();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }

        [Test, TestCaseSource(nameof(GetArgs_RunAdopt))]
        public void PromiseRunIsInvokedAndCompletedProperly_adopt_capture_T(
            SynchronizationType synchronizationType,
            SynchronizationType invokeContext,
            CompleteType completeType,
            bool throwInAction,
            bool isAlreadyComplete)
        {
            bool isPending = !throwInAction & !isAlreadyComplete;

            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;
            string expectedRejectValue = "Reject";
            int expectedResolveValue = 1;
            int captureValue = 10;

            Action tryCompleter = () => { };

            System.Func<int, Promise<int>> action = cv =>
            {
                Assert.AreEqual(captureValue, cv);
                TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                if (throwInAction)
                {
                    if (completeType == CompleteType.Reject)
                    {
                        throw Promise.RejectException(expectedRejectValue);
                    }
                    throw Promise.CancelException();
                }
                return TestHelper.BuildPromise(completeType, isAlreadyComplete, expectedResolveValue, expectedRejectValue, out tryCompleter);
            };

            var promise = default(Promise<int>);
            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                promise = synchronizationType == SynchronizationType.Explicit
                    ? Promise.Run(captureValue, action, TestHelper._foregroundContext)
                    : Promise.Run(captureValue, action, (SynchronizationOption) synchronizationType);
            }, invokeContext == SynchronizationType.Foreground);

            promise
                .ContinueWith(resultContainer =>
                {
                    if (completeType == CompleteType.Reject)
                    {
                        Assert.AreEqual(Promise.State.Rejected, resultContainer.State);
                        Assert.AreEqual(expectedRejectValue, resultContainer.Reason);
                        invoked = true;
                        return;
                    }
                    resultContainer.RethrowIfRejected();
                    if (completeType == CompleteType.Resolve)
                    {
                        Assert.AreEqual(Promise.State.Resolved, resultContainer.State);
                        Assert.AreEqual(expectedResolveValue, resultContainer.Value);
                    }
                    else
                    {
                        Assert.AreEqual(Promise.State.Canceled, resultContainer.State);
                    }
                    invoked = true;
                })
                .Forget();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();

            tryCompleter();

            TestHelper.ExecuteForegroundCallbacksAndWaitForThreadsToComplete();
            Assert.True(invoked);
        }
    }
}