using System.Collections.Concurrent;
using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Core
{

    public class TaskRunnerResult<TResult> : TaskRunnerBase<Func<TResult>>, ITaskRunnerWithResult<TResult>
    {
        private readonly ConcurrentBag<TResult> _results = new ConcurrentBag<TResult>();

        public TaskRunnerResult(string name, Func<TResult> execute, TaskRunnerOptions options)
            : base(name, execute, options)
        {
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var result = Execute();
            _results.Add(result);
            await Task.CompletedTask;
        }

        public IEnumerable<TResult> GetResults()
        {
            return _results.ToArray();
        }

        public void ClearResults()
        {
            _results.Clear();
        }
    }

    public class TaskRunnerResult<T, TResult> : TaskRunnerBase<Func<T, TResult>>, ITaskRunnerWithParam, ITaskRunnerWithResult<TResult>
    {
        private readonly ConcurrentBag<TResult> _results = new ConcurrentBag<TResult>();
        private T Parameter1 { get; set; }

        public TaskRunnerResult(string name, Func<T, TResult> execute, TaskRunnerOptions options, T parameter)
            : base(name, execute, options)
        {
            this.Parameter1 = parameter;
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            var result = Execute(Parameter1);
            _results.Add(result);
            await Task.CompletedTask;
        }

        void ITaskRunnerWithParam.Fire(RequestTaskFire fire, int count, params object[]? parameters)
        {
            if (parameters is { Length: > 0 })
            {
                this.Parameter1 = (T)parameters[0];
            }
            base.Fire(fire, count);
        }

        public IEnumerable<TResult> GetResults()
        {
            return _results.ToArray();
        }

        public void ClearResults()
        {
            _results.Clear();
        }
    }


    public class TaskRunnerResult<T1, T2, TResult> : TaskRunnerBase<Func<T1, T2, TResult>>, ITaskRunnerWithParam, ITaskRunnerWithResult<TResult>
    {
        private readonly ConcurrentBag<TResult> _results = new ConcurrentBag<TResult>();
        private T1 Parameter1 { get; set; }
        private T2 Parameter2 { get; set; }

        public TaskRunnerResult(string name, Func<T1, T2, TResult> execute, TaskRunnerOptions options, T1 parameter1, T2 parameter2)
            : base(name, execute, options)
        {
            this.Parameter1 = parameter1;
            this.Parameter2 = parameter2;
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            var result = Execute(Parameter1, Parameter2);
            _results.Add(result);
            await Task.CompletedTask;
        }

        void ITaskRunnerWithParam.Fire(RequestTaskFire fire, int count, params object[]? parameters)
        {
            if (parameters is { Length: > 0 })
            {
                this.Parameter1 = (T1)parameters[0];
            }
            if (parameters is { Length: > 1 })
            {
                this.Parameter2 = (T2)parameters[1];
            }
            base.Fire(fire, count);
        }

        public IEnumerable<TResult> GetResults()
        {
            return _results.ToArray();
        }

        public void ClearResults()
        {
            _results.Clear();
        }
    }

}
