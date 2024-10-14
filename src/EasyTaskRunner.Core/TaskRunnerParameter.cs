using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Core;

using Data.Interfaces;
public class TaskRunner<T>(string name, Action<T> execute, TaskRunnerOptions options, T parameter)
    : TaskRunnerBase<Action<T>>(name, execute, options), ITaskRunnerWithParam
{
        private T Parameter1 { get; set; } = parameter;

        void ITaskRunnerWithParam.Fire(RequestTaskFire fire, int count, params object[]? parameters)
        {
            if (parameters is { Length: > 0 })
            {
                this.Parameter1 = (T)parameters[0];
            }
            base.Fire(fire, count);
        }
        public void Fire(RequestTaskFire fire, T? parameter1, int count)
        {
            if (parameter1 != null)
            {
                this.Parameter1 = parameter1;
            }

            base.Fire(fire, count);
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            Execute(Parameter1);
            await Task.CompletedTask;
        }
    }

    public class TaskRunner<T1, T2>(string name, Action<T1, T2> execute, TaskRunnerOptions options, T1 parameter1, T2 parameter2)
        : TaskRunnerBase<Action<T1, T2>>(name, execute, options), ITaskRunnerWithParam
    {
        private T1 Parameter1 { get; set; } = parameter1;
        private T2 Parameter2 { get; set; } = parameter2;

        public void Fire(RequestTaskFire fire, T1? parameter1, int count)
        {
            if (parameter1 != null)
            {
                this.Parameter1 = parameter1;
            }

            base.Fire(fire, count);
        }

        public void Fire(RequestTaskFire fire, T1? parameter1, T2? parameter2, int count)
        {
            if (parameter1 != null)
            {
                this.Parameter1 = parameter1;
            }

            if (parameter2 != null)
            {
                this.Parameter2 = parameter2;
            }

            base.Fire(fire, count);
        }

        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            Execute(Parameter1, Parameter2);
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
    }

