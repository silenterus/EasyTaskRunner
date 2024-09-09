using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Core
{
    public abstract class TaskRunnerBase<TAction> : ITaskRunner
        where TAction : Delegate
    {
        private CancellationTokenSource? _ctx;
        private readonly TaskRunnerOptions _options;
        private RequestTaskStatus _taskStatus = RequestTaskStatus.NotStarted;

        protected TaskRunnerBase(string name, TAction execute, TaskRunnerOptions options)
        {
            _options = new TaskRunnerOptions();
            SetOptions(options);

            Name = name;
            Execute = execute;
        }

        private string Name { get; set; }
        private Task? RunningTask { get; set; }

        private int Index { get; set; } = 0;
        private int AllCount { get; set; } = 0;

        private protected TAction Execute { get; set; }

        private bool IsStopping { get; set; } = false;

        private bool IsPaused { get; set; } = false;
        private RequestTaskStatus TaskStatus
        {
            get { return _taskStatus; }
            set
            {
                if (_options.UseLog)
                {
                    // Implement logging if necessary
                }
                _taskStatus = value;
            }
        }

        /*internal protected TaskRunnerBase<TAction> SetOptions(TaskRunnerOptions options)
        {


            return this;
        }*/

        public void GetLogs() { }

        private protected virtual async Task StartRunner(int count)
        {
            _options.SetCount(count);
            await StartRunner();
        }

        private protected virtual async Task StartRunner(int count, int maxParallel)
        {
            _options.SetCounts(count, maxParallel, 0);
            await StartRunner();
        }

        private protected virtual async Task StartRunner(int count, int maxParallel, bool endless)
        {
            _options.SetCounts(count, maxParallel, 0);
            _options.SetEndless(endless);
            await StartRunner();
        }

        private bool TaskCanStart()
        {
            if (RunningTask is { IsCompleted: false, IsCanceled: false })
            {
                return false;
            }
            return true;
        }

        private protected virtual async Task StartRunner()
        {
            if (!TaskCanStart())
                return;

            await Task.Yield();
            IsStopping = false;
            IsPaused = false;
            _options.ErrorCount.Clear(["Error", "Runner"]);

            Index = 0;
            _ctx = new CancellationTokenSource();
            RunningTask = Runner(_ctx.Token, _options.Count, _options.Endless);
            RunningTask.GetAwaiter();
        }

        private async Task UnPauseAsync()
        {
            /*if (TaskStatus != RequestTaskStatus.Paused)
            {
                return;
            }*/
            IsPaused = false;
            await Task.Yield();
        }

        private async Task PauseAsync()
        {
            /*if (TaskStatus != RequestTaskStatus.Running)
            {
                return;
            }*/
            IsPaused = true;
            await Task.Yield();
        }

        private async Task PauseToggle()
        {
            if (IsPaused)
            {
                await UnPauseAsync();
            }
            else
            {
                await PauseAsync();
            }
        }

        protected virtual async Task StopAsync()
        {
            if (RunningTask == null || IsStopping)
                return;

            IsStopping = true;
            IsPaused = false;
            TaskStatus = RequestTaskStatus.Stopping;
            _ctx?.Cancel();

            await WaitAndHandleCompletion();
            TaskStatus = RequestTaskStatus.Stopped;
        }

        private async Task WaitAndHandleCompletion()
        {
            try
            {
                await RunningTask!;
            }
            catch (AggregateException)
            {
                TaskStatus = RequestTaskStatus.Faulted;
            }
        }

        private protected virtual async void Restart()
        {
            _options.ErrorCount.ClearAll();
            await Task.Yield();
            StartAgain();
        }

        private protected virtual async void StartAgain()
        {
            await StopAsync();
            StartRunner().GetAwaiter();
        }

        private async Task Runner(CancellationToken token, int count, bool endless)
        {
            TaskStatus = RequestTaskStatus.Running;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (IsPaused)
                    {
                        TaskStatus = RequestTaskStatus.Paused;
                        i--;
                        continue;
                    }

                    TaskStatus = RequestTaskStatus.Running;
                    if (IsStopping)
                    {
                        TaskStatus = RequestTaskStatus.Stopping;
                        break;
                    }

                    try
                    {
                        ExecuteTask(token);
                        await Task.Delay(_options.Delay, token);
                        Index = i + 1;
                        AllCount++;
                        _options.ErrorCount.Clear("Error");
                    }
                    catch (TaskCanceledException)
                    {
                        IsStopping = true;
                        TaskStatus = RequestTaskStatus.Canceled;
                        break;
                    }
                    catch (Exception)
                    {
                        TaskStatus = RequestTaskStatus.Faulted;
                        _options.ErrorCount.Error(["Runner", "Error", "All"]);
                        if (!_options.ErrorCount.ReachedAll())
                        {
                            continue;
                        }

                        IsStopping = true;
                        TaskStatus = RequestTaskStatus.Faulted;
                        break;
                    }
                }

                if (endless && !IsStopping)
                {
                    StartAgain();
                }
            }
            catch (TaskCanceledException)
            {
                TaskStatus = RequestTaskStatus.Canceled;
            }
            catch (Exception)
            {
                TaskStatus = RequestTaskStatus.Faulted;
            }
            finally
            {
                if (TaskStatus == RequestTaskStatus.Running)
                {
                    TaskStatus = RequestTaskStatus.Completed;
                }
                IsStopping = false;
            }
        }

        public void Fire(RequestTaskFire fire, TaskRunnerOptions options)
        {
            SetOptions(options);
            Fire(fire);
        }

        public string FireWait(RequestTaskFire fire)
        {
            return string.Empty;
        }

        public void Fire(RequestTaskFire fire)
        {
            switch (fire)
            {
                case RequestTaskFire.Start:
                    StartRunner().GetAwaiter();
                    break;

                case RequestTaskFire.Restart:
                    Restart();
                    break;

                case RequestTaskFire.Stop:
                    StopAsync().GetAwaiter();
                    break;

                case RequestTaskFire.Pause:
                    PauseAsync().GetAwaiter();
                    break;
                case RequestTaskFire.UnPause:
                    UnPauseAsync().GetAwaiter();
                    break;
                case RequestTaskFire.Toggle:
                    PauseToggle().GetAwaiter();
                    return;

                case RequestTaskFire.Fire:
                    FireOnce();
                    return;
                case RequestTaskFire.FireWait:
                    FireWait();
                    return;
                case RequestTaskFire.FireEndless:
                    FireEndless();
                    return;
            }
        }

        private void FireOnce()
        {
            _options.SetCount(1).SetMaxParallel(0).SetEndless(false);
            StartRunner().GetAwaiter();
        }

        public void FireEndless()
        {
            _options.SetCount(1).SetMaxParallel(0).SetEndless(true);
            StartRunner().GetAwaiter();
        }

        public void FireWait()
        {
            _options.SetCount(1).SetMaxParallel(0).SetEndless(false);
            StartRunner().Wait();
            Task.Delay(50).Wait();
        }

        public void Fire(RequestTaskFire fire, int count)
        {
            _options.SetCounts(count, 0, 0);
            Fire(fire);
        }

        public void Fire(RequestTaskFire fire, int count, int maxParallel)
        {
            _options.SetCounts(count, maxParallel, maxParallel);
            Fire(fire);
        }

        public void Fire(RequestTaskFire fire, int count, int maxParallel, int maxParallelCount)
        {
            _options.SetCounts(count, maxParallel, maxParallelCount);
            Fire(fire);
        }



        private protected virtual string Status(RequestTaskStatus status, bool useDelay = false)
        {
            if (useDelay)
            {
                Task.Delay(500).Wait();
            }

            return status switch
            {
                RequestTaskStatus.JustInited => $"'{Name}' is idle. - {StatusCount()}",
                RequestTaskStatus.NotStarted => $"'{Name}' not started yet. - {StatusCount()}",
                RequestTaskStatus.Running => $"'{Name}' is currently running. - {StatusCount()}",
                RequestTaskStatus.Stopped => $"'{Name}' is stopped. - {StatusCount()}",
                RequestTaskStatus.Stopping => $"'{Name}' is stopping. - {StatusCount()}",
                RequestTaskStatus.Completed => $"'{Name}' has completed execution. - {StatusCount()}",
                RequestTaskStatus.Canceled => $"'{Name}' was cancelled. - {StatusCount()}",
                RequestTaskStatus.Faulted => $"'{Name}' faulted!!! - {StatusCount()}",
                RequestTaskStatus.Paused => $"'{Name}' is paused. - {StatusCount()}",
                _ => $"Unknown status. - {StatusCount()}"
            };
        }

        /*private protected virtual string Status(RequestTaskStatus status, bool useDelay = false)
        {
            if (useDelay)
            {
                Task.Delay(500).Wait();
            }
            return status switch
            {
                RequestTaskStatus.NotStarted => $"'{Name}' not started yet. - {StatusCount()}",
                RequestTaskStatus.Running => $"'{Name}' is currently running. - {StatusCount()}",
                RequestTaskStatus.Stopped => $"'{Name}' is stopped. - {StatusCount()}",
                RequestTaskStatus.Stopping => $"'{Name}' is stopping. - {StatusCount()}",
                RequestTaskStatus.Completed => $"'{Name}' has completed execution. - {StatusCount()}",
                RequestTaskStatus.Canceled => $"'{Name}' was cancelled. - {StatusCount()}",
                RequestTaskStatus.Faulted => $"'{Name}' faulted!!! - {StatusCount()}",
                RequestTaskStatus.Paused => $"'{Name}' is paused. - {StatusCount()}",
                _ => $"Unknown status. - {StatusCount()}"
            };
        }*/

        public string Status(bool useDelay)
        {
            return Status(TaskStatus,useDelay);
        }
        public string Status()
        {
            return Status(TaskStatus);;
        }

        private string StatusCount()
        {
            return $"[{Index}/{_options.Count}]";
        }



        public RequestTaskStatus GetTaskStatus()
        {
            return TaskStatus;
        }

        public ITaskRunner SetOptions(TaskRunnerOptions options)
        {
            options.Validate();
            _options.CopyFrom(options);
            return this;
        }

        private void ExecuteTask(CancellationToken token)
        {
            if (_options.MaxParallel > 0)
            {
                if (_options.UseSemaphore)
                {
                    ExecuteTaskParallelSemaphor(token, _options.MaxParallel, _options.MaxCount).Wait(token);
                }
                else
                {
                    ExecuteTaskParallel(token, _options.MaxParallel).Wait(token);
                }
            }
            else
            {
                ExecuteTaskAsync(token).Wait(token);
            }
        }

        protected abstract Task ExecuteTaskAsync(CancellationToken token);

        private async Task ExecuteTaskParallel(CancellationToken token, int maxCount)
        {
            var tasks = new Task[maxCount];

            for (int i = 0; i < maxCount; i++)
            {
                tasks[i] = LaunchTaskAsync(token);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                throw new AggregateException("Error occurred while executing tasks in parallel", ex);
            }
        }

        private async Task LaunchTaskAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await Task.Run(() => ExecuteTaskAsync(token), token);
            }
            catch
            {
                throw;
            }
        }

        private async Task ExecuteTaskParallelSemaphor(CancellationToken token, int maxParallel, int maxCount)
        {
            var tasks = new List<Task>();

            using var semaphore = new SemaphoreSlim(maxParallel, maxParallel);

            for (int i = 0; i < maxCount; i++)
            {
                token.ThrowIfCancellationRequested();

                await semaphore.WaitAsync(token);

                tasks.Add(ExecuteTaskWithSemaphoreAsync(semaphore, token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                throw new AggregateException("Error occurred while executing tasks", ex);
            }
        }

        private async Task ExecuteTaskWithSemaphoreAsync(SemaphoreSlim semaphore, CancellationToken token)
        {
            try
            {
                await ExecuteTaskAsync(token);
            }
            finally
            {
                semaphore.Release(); // Ensure semaphore is released even if task fails
            }
        }
    }
}
