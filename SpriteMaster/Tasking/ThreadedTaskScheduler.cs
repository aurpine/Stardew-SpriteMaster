﻿//#define SM_SINGLE_THREAD

using SpriteMaster.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpriteMaster.Tasking;

[DebuggerTypeProxy(typeof(ThreadedTaskSchedulerDebugView))]
[DebuggerDisplay("Id={Id}, ScheduledTasks = {DebugTaskCount}")]
internal sealed class ThreadedTaskScheduler : TaskScheduler, IDisposable {
    internal static readonly ThreadedTaskScheduler Instance = new(useBackgroundThreads: true);

    private class ThreadedTaskSchedulerDebugView {
        private readonly ThreadedTaskScheduler Scheduler;

        public ThreadedTaskSchedulerDebugView(ThreadedTaskScheduler scheduler) {
            Scheduler = scheduler;
        }

        public IEnumerable<Task> ScheduledTasks => Scheduler.PendingTasks;
    }

    private readonly CancellationTokenSource DisposeCancellation = new();
    public int ConcurrencyLevel { get; }

    [ThreadStatic]
    private static bool IsTaskProcessingThread;

    private readonly BlockingCollection<Task> PendingTasks = new();

    private int DebugTaskCount => PendingTasks.Count;

    internal ThreadedTaskScheduler(
        int? concurrencyLevel = null,
        Func<int, string>? threadNameFunction = null,
        bool useBackgroundThreads = false,
        ThreadPriority threadPriority = ThreadPriority.Lowest,
        Action<Thread, int>? onThreadInit = null,
        Action<Thread, int>? onThreadFinally = null
    ) {
        try {
            if (concurrencyLevel is null or 0) {
#if SM_SINGLE_THREAD
			concurrencyLevel = 1;
#else
                concurrencyLevel = Environment.ProcessorCount;
#endif
            }

            concurrencyLevel.Value.AssertPositiveOrZero();

            ConcurrencyLevel = concurrencyLevel.Value;

            for (int i = 0; i < ConcurrencyLevel; ++i) {
                var thread = new Thread(index => DispatchLoop((int)index!, onThreadInit, onThreadFinally)) {
                    Priority = threadPriority,
                    IsBackground = useBackgroundThreads,
                    Name = threadNameFunction is null ? $"ThreadedTaskScheduler Thread {i}" : threadNameFunction(i),
                };
                if (Runtime.IsWindows) {
                    try {
                        thread.SetApartmentState(ApartmentState.MTA);
                    }
                    catch (PlatformNotSupportedException) {
                        /* do nothing */
                    }
                }

                thread.Start(i);
            }
        }
        catch (Exception ex) {
            Debug.Error("Failed to create ThreadedTaskScheduler", ex);
            throw;
        }
    }

    private void DispatchLoop(int index, Action<Thread, int>? onInit, Action<Thread, int>? onFinally) {
        try {
            IsTaskProcessingThread = true;
            var thread = Thread.CurrentThread;
            onInit?.Invoke(thread, index);
            try {
                while (!Config.ForcedDisable) {
                    try {
                        foreach (var task in PendingTasks.GetConsumingEnumerable(DisposeCancellation.Token)) {
                            using var workingState = WatchDog.WatchDog.ScopedWorkingState;
                            if (TryExecuteTask(task) || task.IsCompleted) {
                                task.Dispose();
                            }
                        }
                    }
                    catch (ThreadAbortException) {
                        if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload()) {
                            Thread.ResetAbort();
                        }
                    }
                    catch (ThreadInterruptedException) {
                        // Thread was interrupted by watchdog
                    }
                }
            }
            catch (OperationCanceledException) {
                /* do nothing */
            }
            finally {
                onFinally?.Invoke(thread, index);
            }
        }
        catch (Exception ex) {
            Debug.Error("Error in ThreadedTaskScheduler.DispatchLoop", ex);
            throw;
        }
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    protected override void QueueTask(Task task) {
        if (!Config.IsEnabled) {
            return;
        }

        if (DisposeCancellation.IsCancellationRequested) {
            ThrowHelper.ThrowObjectDisposedException(GetType().Name);
            return;
        }

        PendingTasks.Add(task);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => IsTaskProcessingThread && TryExecuteTask(task);

    protected override IEnumerable<Task> GetScheduledTasks() => PendingTasks.ToArray();

    public void Dispose() => DisposeCancellation.Cancel();
}
