// Licensed to the TOMS Harold-Andreas Zellner.
//  The One Man Solution licenses this file to you under the MIT license.

using EasyTaskRunner.Data.Interfaces;

namespace EasyTaskRunner.Extensions.Helper
{
    using System;
    using System.Threading.Tasks;
    using Data.Enums;
    using Data.Utilities;

    public static class TaskRunnerHelper
    {
        public static TaskRunnerOptions CreateOptions(int count = 1, int maxParallel = 1, int maxCount = 1, bool useSemaphore = false, bool endless = false, int delay = 100)
        {
            return new TaskRunnerOptions(count: count, maxParallel: maxParallel, maxCount: maxCount, useSemaphore: useSemaphore, endless: endless, delay: delay);
        }

        private static TTaskRunner InstantiateTaskRunner<TTaskRunner, TAction>(string name, TAction action, TaskRunnerOptions options)
            where TTaskRunner : ITaskRunner
            where TAction : Delegate
        {
            return (TTaskRunner)Activator.CreateInstance(typeof(TTaskRunner), name, action, options)! ?? throw new InvalidOperationException($"Failed to create an instance of {typeof(TTaskRunner).Name}.");
        }

        public static TTaskRunner Create<TTaskRunner, TAction>(string name, TAction action, TaskRunnerOptions? options = null, int count = 1, int maxParallel = 1, bool endless = false, int delay = 1000)
            where TTaskRunner : ITaskRunner
            where TAction : Delegate
        {
            options ??= CreateOptions(count: count, maxParallel: maxParallel, endless: endless, delay: delay);
            return InstantiateTaskRunner<TTaskRunner, TAction>(name, action, options);
        }

        public static TTaskRunner CreateAndStart<TTaskRunner, TAction>(string name, TAction action, TaskRunnerOptions? options = null, int count = 1, int maxParallel = 1, bool endless = false, int delay = 1000)
            where TTaskRunner : ITaskRunner
            where TAction : Delegate
        {
            var taskRunner = Create<TTaskRunner, TAction>(name, action, options, count, maxParallel, endless, delay);
            taskRunner.Fire(RequestTaskFire.Start);
            return taskRunner;
        }

        public static async Task StopAsync<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            taskRunner.Fire(RequestTaskFire.Stop);
            await Task.Yield();
        }

        public static async Task TogglePauseAsync<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            taskRunner.Fire(RequestTaskFire.Toggle);
            await Task.Yield();
        }

        public static void Restart<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            taskRunner.Fire(RequestTaskFire.Restart);
        }

        public static string GetStatus<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            return taskRunner.Status();
        }

        public static bool IsRunning<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            // Placeholder logic for running status
            return true;
        }

        public static async Task WaitUntilCompletionAsync<TTaskRunner>(TTaskRunner taskRunner)
            where TTaskRunner : ITaskRunner
        {
            while (IsRunning(taskRunner))
            {
                await Task.Delay(100);
            }
        }

        public static TTaskRunner CreateAndFire<TTaskRunner, TAction>(string name, TAction action, RequestTaskFire fire, TaskRunnerOptions options)
            where TTaskRunner : ITaskRunner
            where TAction : Delegate
        {
            var taskRunner = Create<TTaskRunner, TAction>(name, action, options);
            taskRunner.Fire(fire);
            return taskRunner;
        }
    }
}
