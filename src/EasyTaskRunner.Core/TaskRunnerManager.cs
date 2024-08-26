using System.Collections.Concurrent;
using System.Text;
using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
namespace EasyTaskRunner.Core
{

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

        public string Fire(string name, RequestTaskFire fireCommand)
        {
            if (TryGetRunner(name, out var runner))
            {
                runner.Fire(fireCommand);
                return runner.Status();
            }
            return $"Runner '{name}' not found.";
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
            Fire(name, RequestTaskFire.Start);
        }

        public void Stop(string name)
        {
            Fire(name, RequestTaskFire.Stop);
        }

        public void Pause(string name)
        {
            Fire(name, RequestTaskFire.Pause);
        }

        public void Resume(string name)
        {
            Fire(name, RequestTaskFire.UnPause);
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
                statusBuilder.AppendLine($"Runner '{kvp.Key}': {status}");
            }

            return statusBuilder.ToString();
        }
    }
}
