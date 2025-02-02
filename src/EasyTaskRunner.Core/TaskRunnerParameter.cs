namespace EasyTaskRunner.Core
{
    using Data.Interfaces;
    using Data.Enums;
    using Data.Utilities;

    public class TaskRunner<T> : TaskRunnerBase<Action<T>>, ITaskRunnerWithParam
    {
        private T Parameter1 { get; set; }

        public TaskRunner(string name, Action<T> execute, TaskRunnerOptions options, T parameter)
            : base(name, execute, options)
        {
            Parameter1 = parameter;
        }

        public void SetParameter(T parameter)
        {
            Parameter1 = parameter;
        }

        void ITaskRunnerWithParam.Fire(TaskFire fire, int count, params object[]? parameters)
        {
            if (parameters is { Length: > 0 })
            {
                Parameter1 = (T)parameters[0];
            }
            base.Fire(fire, count);
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            Execute(Parameter1);
            await Task.CompletedTask;
        }
    }

    public class TaskRunner<T1, T2> : TaskRunnerBase<Action<T1, T2>>, ITaskRunnerWithParam
    {
        private T1 Parameter1 { get; set; }
        private T2 Parameter2 { get; set; }

        public TaskRunner(string name, Action<T1, T2> execute, TaskRunnerOptions options, T1 parameter1, T2 parameter2)
            : base(name, execute, options)
        {
            Parameter1 = parameter1;
            Parameter2 = parameter2;
        }

        public void SetParameters(T1 parameter1, T2 parameter2)
        {
            Parameter1 = parameter1;
            Parameter2 = parameter2;
        }

        void ITaskRunnerWithParam.Fire(TaskFire fire, int count, params object[]? parameters)
        {
            if (parameters is { Length: > 0 })
            {
                Parameter1 = (T1)parameters[0];
            }
            if (parameters is { Length: > 1 })
            {
                Parameter2 = (T2)parameters[1];
            }
            base.Fire(fire, count);
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            Execute(Parameter1, Parameter2);
            await Task.CompletedTask;
        }
    }
}
