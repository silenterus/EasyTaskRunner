using System.Collections.Concurrent;
using System.Text;
using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;

namespace EasyTaskRunner
{
    using Core;
    using Data.Utilities;
    public class TaskRunnerManager
    {
        private readonly ConcurrentDictionary<string, ITaskRunner> _runners = new ConcurrentDictionary<string, ITaskRunner>();


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
            var runner = new TaskRunner<T>(name, execute, options ?? new TaskRunnerOptions(), parameter);
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T1, T2>(string name, Action<T1, T2> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunner<T1, T2>(name, execute, options ?? new TaskRunnerOptions(), parameter1, parameter2);
            return _runners.TryAdd(name, runner);
        }

        public bool Add<TResult>(string name, Func<TResult> execute, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<TResult>(name, execute, options ?? new TaskRunnerOptions());
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T, TResult>(string name, Func<T, TResult> execute, T parameter, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<T, TResult>(name, execute, options ?? new TaskRunnerOptions(), parameter);
            return _runners.TryAdd(name, runner);
        }

        public bool Add<T1, T2, TResult>(string name, Func<T1, T2, TResult> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
        {
            var runner = new TaskRunnerResult<T1, T2, TResult>(name, execute, options ?? new TaskRunnerOptions(), parameter1, parameter2);
            return _runners.TryAdd(name, runner);
        }


        public string Fire(string name, RequestTaskFire fireCommand, int count, params object[]? parameters)
        {
            if (!TryGetRunner(name, out var runner))
            {
                return $"Runner '{name}' not found.";
            }

            if (runner is ITaskRunnerWithParam runnerWithParam)
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

            if (runner is ITaskRunnerWithResult<TResult> runnerWithResult)
            {
                return runnerWithResult.GetResults();
            }
            else
            {
                throw new InvalidOperationException($"Runner '{name}' does not return results of type {typeof(TResult).Name}.");
            }
        }

        public void ClearResults(string name)
        {
            if (TryGetRunner(name, out var runner))
            {
                if (runner is ITaskRunnerWithResult<object> runnerWithResult)
                {
                    runnerWithResult.ClearResults();
                }
                else
                {
                    throw new InvalidOperationException($"Runner '{name}' does not support clearing results.");
                }
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
            Fire(name, RequestTaskFire.Start,0);
        }

        public void Stop(string name)
        {
            Fire(name, RequestTaskFire.Stop,0);
        }

        public void Pause(string name)
        {
            Fire(name, RequestTaskFire.Pause,0);
        }

        public void Resume(string name)
        {
            Fire(name, RequestTaskFire.UnPause,0);
        }

        public void StartAll()
        {
            FireAll(RequestTaskFire.Start);
        }

        public void StopAll()
        {
            FireAll(RequestTaskFire.Stop);
        }

        public void PauseAll()
        {
            FireAll(RequestTaskFire.Pause);
        }

        public void ResumeAll()
        {
            FireAll(RequestTaskFire.UnPause);
        }

        private bool TryGetRunner(string name, out ITaskRunner runner)
        {
            return _runners.TryGetValue(name, out runner);
        }

        private void FireAll(RequestTaskFire fireCommand)
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
                statusBuilder.AppendLine($"{status}");
            }

            return statusBuilder.ToString();
        }
    }
}
