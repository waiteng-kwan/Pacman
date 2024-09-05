﻿#if !UNITY_WEBGL

#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

using NUnit.Framework;
using Proto.Promises;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProtoPromiseTests.Concurrency.PromiseGroups
{
    public class PromiseMergeResultsGroupConcurrencyTests
    {
        const string rejectValue = "Fail";

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
        public void DeferredsMayBeCompletedWhileTheirPromisesArePassedToPromiseMergeResultsGroup_AndCancelationTriggeredConcurrently_T_void(
            [Values] bool withCancelation,
            [Values] CombineType combineType,
            [Values] CompleteType completeType1,
            [Values] bool alreadyComplete1,
            [Values] CompleteType completeType2,
            // We need at least 1 promise to be pending.
            [Values(false)] bool alreadyComplete2)
        {
            var tryCompleter1 = default(Action);
            var tryCompleter2 = default(Action);
            var promise1 = default(Promise<int>);
            var promise2 = default(Promise);
            var cancelationSource = default(CancelationSource);
            var groupCancelationToken = default(CancelationToken);
            var group = default(PromiseMergeResultsGroup);

            List<Action> parallelActions = new List<Action>();
            if (!alreadyComplete1)
            {
                parallelActions.Add(() => tryCompleter1());
            }
            if (!alreadyComplete2)
            {
                parallelActions.Add(() => tryCompleter2());
            }
            if (withCancelation)
            {
                parallelActions.Add(() => cancelationSource.Cancel());
            }

            var helper = ParallelCombineTestHelper.Create(
                combineType,
                () => group
                    .Add(promise1)
                    .Add(promise2)
                    .WaitAsync()
            );
            helper.MaybeAddParallelAction(parallelActions);

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsMaybeWithOffsets(
                // setup
                () =>
                {
                    if (withCancelation)
                    {
                        cancelationSource = CancelationSource.New();
                        group = PromiseMergeResultsGroup.New(cancelationSource.Token, out groupCancelationToken);
                    }
                    else
                    {
                        group = PromiseMergeResultsGroup.New(out groupCancelationToken);
                    }
                    promise1 = TestHelper.BuildPromise(completeType1, alreadyComplete1, 1, rejectValue, groupCancelationToken, out tryCompleter1);
                    promise2 = TestHelper.BuildPromise(completeType2, alreadyComplete2, rejectValue, groupCancelationToken, out tryCompleter2);
                    helper.Setup();
                },
                // teardown
                () =>
                {
                    helper.Teardown();
                    cancelationSource.TryDispose();
                    Assert.IsTrue(helper.Success);
                },
                parallelActions
            );
        }

        [Test]
        public void DeferredsMayBeCompletedWhileTheirPromisesArePassedToPromiseMergeResultsGroup_AndCancelationTriggeredConcurrently_void_T_void_T(
            [Values] bool withCancelation,
            [Values] CombineType combineType,
            [Values] CompleteType completeType1,
            [Values] bool alreadyComplete1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            // We need at least 1 promise to be pending.
            [Values(false)] bool alreadyComplete2,
            [Values(CompleteType.Resolve)] CompleteType completeType3,
            // Only have up to 2 already complete to reduce test count.
            [Values(false)] bool alreadyComplete3,
            [Values] CompleteType completeType4,
            [Values] bool alreadyComplete4)
        {
            var tryCompleter1 = default(Action);
            var tryCompleter2 = default(Action);
            var tryCompleter3 = default(Action);
            var tryCompleter4 = default(Action);
            var promise1 = default(Promise);
            var promise2 = default(Promise<int>);
            var promise3 = default(Promise);
            var promise4 = default(Promise<int>);
            var cancelationSource = default(CancelationSource);
            var groupCancelationToken = default(CancelationToken);
            var group = default(PromiseMergeResultsGroup);

            List<Action> parallelActions = new List<Action>();
            if (!alreadyComplete1)
            {
                parallelActions.Add(() => tryCompleter1());
            }
            if (!alreadyComplete2)
            {
                parallelActions.Add(() => tryCompleter2());
            }
            if (!alreadyComplete3)
            {
                parallelActions.Add(() => tryCompleter3());
            }
            if (!alreadyComplete4)
            {
                parallelActions.Add(() => tryCompleter4());
            }
            if (withCancelation)
            {
                parallelActions.Add(() => cancelationSource.Cancel());
            }

            var helper = ParallelCombineTestHelper.Create(
                combineType,
                () => group
                    .Add(promise1)
                    .Add(promise2)
                    .Add(promise3)
                    .Add(promise4)
                    .WaitAsync()
            );
            helper.MaybeAddParallelAction(parallelActions);

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsMaybeWithOffsets(
                // setup
                () =>
                {
                    if (withCancelation)
                    {
                        cancelationSource = CancelationSource.New();
                        group = PromiseMergeResultsGroup.New(cancelationSource.Token, out groupCancelationToken);
                    }
                    else
                    {
                        group = PromiseMergeResultsGroup.New(out groupCancelationToken);
                    }
                    promise1 = TestHelper.BuildPromise(completeType1, alreadyComplete1, rejectValue, groupCancelationToken, out tryCompleter1);
                    promise2 = TestHelper.BuildPromise(completeType2, alreadyComplete2, 1, rejectValue, groupCancelationToken, out tryCompleter2);
                    promise3 = TestHelper.BuildPromise(completeType3, alreadyComplete3, rejectValue, groupCancelationToken, out tryCompleter3);
                    promise4 = TestHelper.BuildPromise(completeType4, alreadyComplete4, 2, rejectValue, groupCancelationToken, out tryCompleter4);
                    helper.Setup();
                },
                // teardown
                () =>
                {
                    helper.Teardown();
                    cancelationSource.TryDispose();
                    Assert.IsTrue(helper.Success);
                },
                parallelActions
            );
        }
    }
}

#endif // !UNITY_WEBGL