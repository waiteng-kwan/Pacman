﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using System.Linq;
using Proto.Promises;
using NUnit.Framework;

namespace ProtoPromiseTests.APIs
{
    public class AllTests
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
        public void AllPromiseIsResolvedWhenAllPromisesAreResolved_void()
        {
            var deferred1 = Promise.NewDeferred();
            var deferred2 = Promise.NewDeferred();

            bool invoked = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(() => { invoked = true; })
                .Forget();

            Assert.IsFalse(invoked);

            deferred1.Resolve();
            Assert.IsFalse(invoked);

            deferred2.Resolve();
            Assert.IsTrue(invoked);
        }

        [Test]
        public void AllPromiseIsResolvedWhenAllPromisesAreResolved_T()
        {
            var deferred1 = Promise.NewDeferred<int>();
            var deferred2 = Promise.NewDeferred<int>();

            bool invoked = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(values =>
                {
                    invoked = true;
                    Assert.AreEqual(2, values.Count);
                    Assert.AreEqual(10, values[0]);
                    Assert.AreEqual(20, values[1]);
                })
                .Forget();

            Assert.IsFalse(invoked);

            deferred1.Resolve(10);

            Assert.IsFalse(invoked);

            deferred2.Resolve(20);

            Assert.IsTrue(invoked);
        }

        [Test]
        public void AllPromiseIsResolvedIfThereAreNoPromises_void()
        {
            bool invoked = false;

            Promise.All(Enumerable.Empty<Promise>())
                .Then(() => { invoked = true; })
                .Forget();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void AllPromiseIsResolvedIfThereAreNoPromises_T()
        {
            bool invoked = false;

            Promise<int>.All(Enumerable.Empty<Promise<int>>())
                .Then(v =>
                {
                    invoked = true;
                    Assert.IsEmpty(v);
                })
                .Forget();

            Assert.IsTrue(invoked);
        }


        [Test]
        public void AllPromiseIsResolvedWhenAllPromisesAreAlreadyResolved_void()
        {
            bool invoked = false;

            Promise.All(Promise.Resolved(), Promise.Resolved())
                .Then(() => { invoked = true; })
                .Forget();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void AllPromiseIsResolvedWhenAllPromisesAreAlreadyResolved_T()
        {
            bool invoked = false;

            Promise<int>.All(Promise.Resolved(1), Promise.Resolved(2))
                .Then(v => { invoked = true; })
                .Forget();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void AllPromiseIsRejectedWhenFirstPromiseIsRejected_void()
        {
            var deferred1 = Promise.NewDeferred();
            var deferred2 = Promise.NewDeferred();

            bool errored = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Reject("Error!");

            Assert.IsTrue(errored);

            deferred2.Resolve();

            Assert.IsTrue(errored);
        }

        [Test]
        public void AllPromiseIsRejectedWhenFirstPromiseIsRejected_T()
        {
            var deferred1 = Promise.NewDeferred<int>();
            var deferred2 = Promise.NewDeferred<int>();

            bool errored = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Reject("Error!");

            Assert.IsTrue(errored);

            deferred2.Resolve(2);

            Assert.IsTrue(errored);
        }

        [Test]
        public void AllPromiseIsRejectedWhenSecondPromiseIsRejected_void()
        {
            var deferred1 = Promise.NewDeferred();
            var deferred2 = Promise.NewDeferred();

            bool errored = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Resolve();

            Assert.IsFalse(errored);

            deferred2.Reject("Error!");

            Assert.IsTrue(errored);
        }

        [Test]
        public void AllPromiseIsRejectedWhenSecondPromiseIsRejected_T()
        {
            var deferred1 = Promise.NewDeferred<int>();
            var deferred2 = Promise.NewDeferred<int>();

            bool errored = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Resolve(2);

            Assert.IsFalse(errored);

            deferred2.Reject("Error!");

            Assert.IsTrue(errored);
        }

        [Test]
        public void AllPromiseIsRejectedWhenBothPromisesAreRejected_void()
        {
            // All does not suppress rejections if one of the promises is rejected before the others complete.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            bool uncaughtHandled = false;
            Promise.Config.UncaughtRejectionHandler = e =>
            {
                uncaughtHandled = true;
                Assert.AreEqual("Error 2", e.Value);
            };

            var deferred1 = Promise.NewDeferred();
            var deferred2 = Promise.NewDeferred();

            bool errored = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Reject("Error 1");

            Assert.IsTrue(errored);

            deferred2.Reject("Error 2");

            Assert.IsTrue(errored);

            Assert.True(uncaughtHandled);
            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test]
        public void AllPromiseIsRejectedWhenBothPromisesAreRejected_T()
        {
            // All does not suppress rejections if one of the promises is rejected before the others complete.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            bool uncaughtHandled = false;
            Promise.Config.UncaughtRejectionHandler = e =>
            {
                uncaughtHandled = true;
                Assert.AreEqual("Error 2", e.Value);
            };

            var deferred1 = Promise.NewDeferred<int>();
            var deferred2 = Promise.NewDeferred<int>();

            bool errored = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    (string e) => { errored = true; })
                .Forget();

            deferred1.Reject("Error 1");

            Assert.IsTrue(errored);

            deferred2.Reject("Error 2");

            Assert.IsTrue(errored);

            Assert.True(uncaughtHandled);
            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test]
        public void AllPromiseIsRejectedWhenAnyPromiseIsAlreadyRejected_void()
        {
            int rejectCount = 0;
            string rejection = "Error!";

            var deferred = Promise.NewDeferred();
            using (var promiseRetainer1 = deferred.Promise.GetRetainer())
            {
                using (var promiseRetainer2 = Promise.Rejected(rejection).GetRetainer())
                {
                    Promise.All(promiseRetainer1.WaitAsync(), promiseRetainer2.WaitAsync())
                        .Then(
                            () => Assert.Fail("Promise was resolved when it should have been rejected."),
                            (string ex) =>
                            {
                                Assert.AreEqual(rejection, ex);
                                ++rejectCount;
                            })
                        .Forget();

                    Promise.All(promiseRetainer2.WaitAsync(), promiseRetainer1.WaitAsync())
                        .Then(
                            () => Assert.Fail("Promise was resolved when it should have been rejected."),
                            (string ex) =>
                            {
                                Assert.AreEqual(rejection, ex);
                                ++rejectCount;
                            })
                        .Forget();

                    Assert.AreEqual(2, rejectCount);

                    deferred.Resolve();

                    Assert.AreEqual(2, rejectCount);
                }
            }
        }

        [Test]
        public void AllPromiseIsRejectedWhenAnyPromiseIsAlreadyRejected_T()
        {
            int rejectCount = 0;
            string rejection = "Error!";

            var deferred = Promise.NewDeferred<int>();
            using (var promiseRetainer1 = deferred.Promise.GetRetainer())
            {
                using (var promiseRetainer2 = Promise<int>.Rejected(rejection).GetRetainer())
                {
                    Promise<int>.All(promiseRetainer1.WaitAsync(), promiseRetainer2.WaitAsync())
                        .Then(
                            v => Assert.Fail("Promise was resolved when it should have been rejected."),
                            (string ex) =>
                            {
                                Assert.AreEqual(rejection, ex);
                                ++rejectCount;
                            })
                        .Forget();

                    Promise<int>.All(promiseRetainer2.WaitAsync(), promiseRetainer1.WaitAsync())
                        .Then(
                            v => Assert.Fail("Promise was resolved when it should have been rejected."),
                            (string ex) =>
                            {
                                Assert.AreEqual(rejection, ex);
                                ++rejectCount;
                            })
                        .Forget();

                    Assert.AreEqual(2, rejectCount);

                    deferred.Resolve(1);

                    Assert.AreEqual(2, rejectCount);
                }
            }
        }

        [Test]
        public void AllPromiseIsCanceledWhenFirstPromiseIsCanceled_void()
        {
            CancelationSource cancelationSource = CancelationSource.New();

            var deferred1 = Promise.NewDeferred();
            cancelationSource.Token.Register(deferred1);
            var deferred2 = Promise.NewDeferred();

            bool canceled = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(() => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            cancelationSource.Cancel();

            Assert.IsTrue(canceled);

            deferred2.Resolve();

            Assert.IsTrue(canceled);

            cancelationSource.Dispose();
        }

        [Test]
        public void AllPromiseIsCanceledWhenFirstPromiseIsCanceled_T()
        {
            CancelationSource cancelationSource = CancelationSource.New();

            var deferred1 = Promise.NewDeferred<int>();
            cancelationSource.Token.Register(deferred1);
            var deferred2 = Promise.NewDeferred<int>();

            bool canceled = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(v => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            cancelationSource.Cancel();

            Assert.IsTrue(canceled);

            deferred2.Resolve(2);

            Assert.IsTrue(canceled);

            cancelationSource.Dispose();
        }

        [Test]
        public void AllPromiseIsCanceledWhenSecondPromiseIsCanceled_void()
        {
            var deferred1 = Promise.NewDeferred();
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred2 = Promise.NewDeferred();
            cancelationSource.Token.Register(deferred2);

            bool canceled = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(() => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            deferred1.Resolve();

            Assert.IsFalse(canceled);

            cancelationSource.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource.Dispose();
        }

        [Test]
        public void AllPromiseIsCanceledWhenSecondPromiseIsCanceled_T()
        {
            var deferred1 = Promise.NewDeferred<int>();
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred2 = Promise.NewDeferred<int>();
            cancelationSource.Token.Register(deferred2);

            bool canceled = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(v => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            deferred1.Resolve(2);

            Assert.IsFalse(canceled);

            cancelationSource.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource.Dispose();
        }

        [Test]
        public void AllPromiseIsCanceledWhenBothPromisesAreCanceled_void()
        {
            CancelationSource cancelationSource1 = CancelationSource.New();
            CancelationSource cancelationSource2 = CancelationSource.New();

            var deferred1 = Promise.NewDeferred();
            cancelationSource1.Token.Register(deferred1);
            var deferred2 = Promise.NewDeferred();
            cancelationSource2.Token.Register(deferred2);

            bool canceled = false;

            Promise.All(deferred1.Promise, deferred2.Promise)
                .Then(() => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            cancelationSource1.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource2.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource1.Dispose();
            cancelationSource2.Dispose();
        }

        [Test]
        public void AllPromiseIsCanceledWhenBothPromisesAreCanceled_T()
        {
            CancelationSource cancelationSource1 = CancelationSource.New();
            CancelationSource cancelationSource2 = CancelationSource.New();

            var deferred1 = Promise.NewDeferred<int>();
            cancelationSource1.Token.Register(deferred1);
            var deferred2 = Promise.NewDeferred<int>();
            cancelationSource2.Token.Register(deferred2);

            bool canceled = false;

            Promise<int>.All(deferred1.Promise, deferred2.Promise)
                .Then(v => Assert.Fail("Promise was resolved when it should have been canceled."))
                .CatchCancelation(() => { canceled = true; })
                .Forget();

            cancelationSource1.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource2.Cancel();

            Assert.IsTrue(canceled);

            cancelationSource1.Dispose();
            cancelationSource2.Dispose();
        }

        [Test]
        public void AllPromiseIsCancelededWhenAnyPromiseIsAlreadyCanceled_void()
        {
            int cancelCount = 0;

            var deferred = Promise.NewDeferred();
            using (var promiseRetainer1 = deferred.Promise.GetRetainer())
            {
                using (var promiseRetainer2 = Promise.Canceled().GetRetainer())
                {
                    Promise.All(promiseRetainer1.WaitAsync(), promiseRetainer2.WaitAsync())
                        .Then(() => Assert.Fail("Promise was resolved when it should have been canceled."))
                        .CatchCancelation(() =>
                        {
                            ++cancelCount;
                        })
                        .Forget();

                    Promise.All(promiseRetainer2.WaitAsync(), promiseRetainer1.WaitAsync())
                        .Then(() => Assert.Fail("Promise was resolved when it should have been canceled."))
                        .CatchCancelation(() =>
                        {
                            ++cancelCount;
                        })
                        .Forget();

                    deferred.Resolve();

                    Assert.AreEqual(2, cancelCount);
                }
            }
        }

        [Test]
        public void AllPromiseIsCancelededWhenAnyPromiseIsAlreadyCanceled_T()
        {
            int cancelCount = 0;

            var deferred = Promise.NewDeferred<int>();
            using (var promiseRetainer1 = deferred.Promise.GetRetainer())
            {
                using (var promiseRetainer2 = Promise<int>.Canceled().GetRetainer())
                {
                    Promise<int>.All(promiseRetainer1.WaitAsync(), promiseRetainer2.WaitAsync())
                        .Then(v => Assert.Fail("Promise was resolved when it should have been canceled."))
                        .CatchCancelation(() =>
                        {
                            ++cancelCount;
                        })
                        .Forget();

                    Promise<int>.All(promiseRetainer2.WaitAsync(), promiseRetainer1.WaitAsync())
                        .Then(v => Assert.Fail("Promise was resolved when it should have been canceled."))
                        .CatchCancelation(() =>
                        {
                            ++cancelCount;
                        })
                        .Forget();

                    deferred.Resolve(1);

                    Assert.AreEqual(2, cancelCount);
                }
            }
        }
    }
}