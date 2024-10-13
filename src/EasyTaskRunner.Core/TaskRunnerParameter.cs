using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Core;

using Data.Interfaces;
public class TaskRunner<T> : TaskRunnerBase<Action<T>>, ITaskRunnerWithParam
    {
        private T Parameter1 { get; set; }

        public TaskRunner(string name, Action<T> execute, TaskRunnerOptions options, T parameter)
            : base(name, execute, options)
        {
            this.Parameter1 = parameter;
        }
        // Implement the ITaskRunnerWithParam interface
        void ITaskRunnerWithParam.Fire(RequestTaskFire fire, int count, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
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

    public class TaskRunner<T1, T2> : TaskRunnerBase<Action<T1, T2>>, ITaskRunnerWithParam
    {
        private T1 Parameter1 { get; set; }
        private T2 Parameter2 { get; set; }

        public TaskRunner(string name, Action<T1, T2> execute, TaskRunnerOptions options, T1 parameter1, T2 parameter2)
            : base(name, execute, options)
        {
            this.Parameter1 = parameter1;
            this.Parameter2 = parameter2;
        }

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


        // Implement the ITaskRunnerWithParam interface
        void ITaskRunnerWithParam.Fire(RequestTaskFire fire, int count, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                this.Parameter1 = (T1)parameters[0];
            }
            if (parameters != null && parameters.Length > 1)
            {
                this.Parameter2 = (T2)parameters[1];
            }
            base.Fire(fire, count);
        }
    }

