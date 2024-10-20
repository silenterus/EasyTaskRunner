using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Core
{
    public class TaskRunner : TaskRunnerBase<Action>, ITaskRunner
    {
        public TaskRunner(string name, Action execute, TaskRunnerOptions options)
            : base(name, execute, options) { }
        public TaskRunner(string name, Action execute) :  base(name, execute, new TaskRunnerOptions(1))
        {
        }
        protected override async Task ExecuteTaskAsync(CancellationToken token)
        {
            await Task.Yield();
            Execute();
        }


        ITaskRunner ITaskRunner.SetOptions(TaskRunnerOptions options) => this.SetOptions(options);

        //public ITaskRunner SetOptions(TaskRunnerOptions options) => null;

    }



}
