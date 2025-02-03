using System.Collections.Concurrent;
using System.Text;


namespace EasyTaskRunner
{
    using Core;
    using Data.Events;
    using Data.Utilities;

    using EasyTaskRunner.Data.Enums;
    using EasyTaskRunner.Data.Interfaces;
    public class TaskRunnerManager
    {
        private readonly ConcurrentDictionary<string, ITaskRunner> _runners = new ConcurrentDictionary<string, ITaskRunner>();

        public event EventHandler<TaskEventArgs>? TaskRegistered;
        public event EventHandler<TaskEventArgs>? TaskUnregistered;

        public event EventHandler<TaskEventArgs>? ResultAdded;
        public event EventHandler<TaskEventArgs>? ParameterChanged;

        public event EventHandler<TaskEventArgs>? OnSucceed;
        public event EventHandler<TaskEventArgs>? OnError;

        public event EventHandler<TaskEventArgs>? OnAbort;


        private TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _monitoringTask;
        private readonly object _monitorLock = new object();
        private readonly Dictionary<string, ITaskRunner> _taskSnapshot = new Dictionary<string, ITaskRunner>();


        public TaskRunnerManager(bool startMonitor = false, float pollingInterval = 1f)
        {
            if (pollingInterval > 0)
            {
                _pollingInterval = TimeSpan.FromSeconds(pollingInterval);
            }

            if (startMonitor)
            {
                StartMonitoring();
            }
        }
        public void StopMonitoring()
        {
            lock (_monitorLock)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("Monitoring is already stopped.");
                    return;
                }

                _cancellationTokenSource.Cancel();
                Console.WriteLine("Stopping monitoring tasks.");

                try
                {
                    _monitoringTask?.Wait();
                    Console.WriteLine("Monitoring tasks stopped.");
                }
                catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
                {
                    // Expected when the task is canceled
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                    _monitoringTask = null;
                }
            }
        }

        public void StartMonitoring()
        {
            lock (_monitorLock)
            {
                if (_monitoringTask != null && !_monitoringTask.IsCompleted)
                {
                    Console.WriteLine("Monitoring is already running.");
                    return;
                }

                // Dispose the previous token source if it's not already cancelled
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                // Create a new CancellationTokenSource for the new monitoring task
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;

                // Start the monitoring task
                _monitoringTask = Task.Run(() => MonitorTaskChangesAsync(token), token);
                Console.WriteLine("Started monitoring tasks.");
            }
        }

        public bool RegisterTask(string name, ITaskRunner runner)
        {
            if (!_runners.TryAdd(name, runner))
            {
                return false;
            }

            _taskSnapshot[name] = runner;
            TaskRegistered?.Invoke(this, new TaskEventArgs(name, runner));
            return true;
        }

        public bool UnregisterTask(string name)
        {
            if (!_runners.TryRemove(name, out var runner))
            {
                return false;
            }

            runner.Fire(TaskFire.Stop);
            _taskSnapshot.Remove(name);
            TaskUnregistered?.Invoke(this, new TaskEventArgs(name, runner));
            return true;
        }

        private async Task MonitorTaskChangesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_pollingInterval, token);

                    var currentTasks = _runners.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    foreach (var kvp in currentTasks)
                    {
                        if (_taskSnapshot.ContainsKey(kvp.Key))
                        {
                            continue;
                        }

                        OnTaskChanged(kvp.Key, null, kvp.Value);
                        _taskSnapshot[kvp.Key] = kvp.Value;
                    }

                    var removedTasks = _taskSnapshot.Keys.Except(currentTasks.Keys).ToList();
                    foreach (var name in removedTasks)
                    {
                        OnTaskChanged(name, _taskSnapshot[name], null);
                        _taskSnapshot.Remove(name);
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in MonitorTaskChangesAsync: " + ex.Message);
                }
            }
        }

        private void OnTaskChanged(string taskName, ITaskRunner? oldTask, ITaskRunner? newTask)
        {
            if (oldTask == null && newTask != null)
            {
                Console.WriteLine($"Task '{taskName}' has been registered.");
                TaskRegistered?.Invoke(this, new TaskEventArgs(taskName, newTask));
            }
            else if (oldTask != null && newTask == null)
            {
                Console.WriteLine($"Task '{taskName}' has been unregistered.");
                TaskUnregistered?.Invoke(this, new TaskEventArgs(taskName, oldTask));
            }
            else
            {
                // Task updated (if necessary)
            }
        }


        public bool Add(string name, ITaskRunner runner)
        {
            return _runners.TryAdd(name, runner);
        }

        public bool Remove(string name)
        {
            return _runners.TryRemove(name, out _);
        }

        public ITaskRunner? Get(string name)
        {
            _runners.TryGetValue(name, out var runner);
            return runner;
        }



        public bool Add(string name, Action execute, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunner(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T>(string name, Action<T> execute, T parameter, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunner<T>(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T1, T2>(string name, Action<T1, T2> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunner<T1, T2>(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<TResult>(string name, Func<TResult> execute, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<TResult>(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T, TResult>(string name, Func<T, TResult> execute, T parameter, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<T, TResult>(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T1, T2, TResult>(string name, Func<T1, T2, TResult> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<T1, T2, TResult>(name, execute, options ?? new TaskRunnerOptions(), parameter1, parameter2);
            return _runners.TryAdd(name, runner);
        }

        public string Fire(string name, TaskFire fireCommand, int count, params object[]? parameters)
        {
            if (!TryGetRunner(name, out var runner))
            {
                return $"Runner '{name}' not found.";
            }

            if (runner is ITaskRunnerParam runnerWithParam)
            {
                if (parameters != null)
                {
                    runnerWithParam.Fire(fireCommand, count, parameters);
                }
            }

            else
            {
                runner.Fire(fireCommand, count);
            }
            return runner.Status();
        }

        public IEnumerable<TResult> GetResults<TResult>(string name)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }

            if (runner is ITaskRunnerResult<TResult> runnerWithResult)
            {
                return runnerWithResult.GetResults();
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not return results of type {typeof(TResult).Name}.");
            }
        }

        public void ClearAll()
        {
            foreach (var runner in _runners.Values)
            {
                if (runner is ITaskRunnerResult<object> runnerWithResult)
                {
                    runnerWithResult.ClearResults();
                }
            }
        }

        public void ClearResults(string name)
        {
            if (!TryGetRunner(name, out var runner))
            {
                return;
            }

            if (runner is ITaskRunnerResult<object> runnerWithResult)
            {
                runnerWithResult.ClearResults();
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not support clearing results.");
            }
        }



        public string GetStatus(string name)
        {
            if (TryGetRunner(name, out var runner))
            {
                return runner.Status();
            }
            return $"Runner '{name}' not found.";
        }

        public void Start(string name)
        {
            Fire(name, TaskFire.Start,0);
        }

        public void Stop(string name)
        {
            Fire(name, TaskFire.Stop,0);
        }

        public void Pause(string name)
        {
            Fire(name, TaskFire.Pause,0);
        }

        public void Resume(string name)
        {
            Fire(name, TaskFire.UnPause,0);
        }

        public void StartAll()
        {
            FireAll(TaskFire.Start);
        }

        public void StopAll()
        {
            FireAll(TaskFire.Stop);
        }

        public void PauseAll()
        {
            FireAll(TaskFire.Pause);
        }

        public void ResumeAll()
        {
            FireAll(TaskFire.UnPause);
        }

        private bool TryGetRunner(string name, out ITaskRunner runner)
        {
            return _runners.TryGetValue(name, out runner);
        }

        private void FireAll(TaskFire fireCommand)
        {
            foreach (var runner in _runners.Values)
            {
                runner.Fire(fireCommand);
            }
        }



        public string GetStatusAll()
        {
            var statusBuilder = new StringBuilder();

            foreach (var kvp in _runners)
            {
                string status = kvp.Value.Status();
                statusBuilder.AppendLine($"{kvp.Key}:[{status}] [{kvp.Value.GetTaskStatus()}]");
            }

            return statusBuilder.ToString();
        }




        public void Fire(string name, TaskFire fire)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.Fire(fire);
        }

        public void Fire(string name, TaskFire fire, TaskRunnerOptions options)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.Fire(fire, options);
        }

        public void Fire(string name, TaskFire fire, int count)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.Fire(fire, count);
        }

        public void Fire(string name, TaskFire fire, int count, int maxParallel)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.Fire(fire, count, maxParallel);
        }

        public void Fire(string name, TaskFire fire, int count, int maxParallel, int maxParallelCount)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.Fire(fire, count, maxParallel, maxParallelCount);
        }



        public void SetParam<T>(string name, T parameter)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }

            if (runner is TaskRunner<T> runnerWithParam)
            {
                runnerWithParam.SetParameter(parameter);
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not accept a parameter of type {typeof(T).Name}.");
            }
        }

        public void SetParam<T1, T2, T3>(string name, T1 parameter1, T2 parameter2)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }
            if (runner is TaskRunnerResult<T1, T2,T3> runnerWithResult)
            {
                runnerWithResult.SetParameters(parameter1,parameter2);
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not accept parameters of types {typeof(T1).Name}, {typeof(T2).Name}.");
            }
        }


        public void SetParam<T1, T2>(string name, T1 parameter1, T2 parameter2)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }

            if (runner is TaskRunner<T1, T2> runnerWithParams)
            {
                runnerWithParams.SetParameters(parameter1, parameter2);
            }
            else if (runner is TaskRunnerResult<T1, T2> runnerWithResult)
            {
                runnerWithResult.SetParameters(parameter1);
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not accept parameters of types {typeof(T1).Name}, {typeof(T2).Name}.");
            }
        }

        public void SetParam<T>(string name, Func<T> parameterProvider)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }

            if (runner is TaskRunner<T> runnerWithParam)
            {
                runnerWithParam.SetParameter(parameterProvider());
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not accept a parameter of type {typeof(T).Name}.");
            }
        }

        public void SetParam<T1, T2>(string name, Func<T1> parameterProvider1, Func<T2> parameterProvider2)
        {
            if (!TryGetRunner(name, out var runner))
            {
                throw new KeyNotFoundException($"Runner '{name}' not found.");
            }

            if (runner is TaskRunner<T1, T2> runnerWithParams)
            {
                runnerWithParams.SetParameters(parameterProvider1(), parameterProvider2());
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not accept parameters of types {typeof(T1).Name}, {typeof(T2).Name}.");
            }
        }
        public void SetOptions(string name, TaskRunnerOptions options)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.SetOptions(options);
        }

        public void GetLogs(string name)
        {
            if (!TryGetRunner(name, out var runner))
            {
                Console.WriteLine($"Runner '{name}' not found.");
                return;
            }

            runner.GetLogs();
        }




    }
}
