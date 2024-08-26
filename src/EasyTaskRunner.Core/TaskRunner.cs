using System.Collections.Concurrent;
using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
using EasyTaskRunner.Data.Utilities;
namespace EasyTaskRunner.Core;

// Concrete implementation for Action
public class TaskRunner : TaskRunnerBase<Action>, ITaskRunner
{
    public TaskRunner(string name, Action execute, TaskRunnerOptions options) :  base(name, execute, options)
    {
    }

    protected override async Task ExecuteTaskAsync(CancellationToken token)
    {
        await Task.Yield();
        Execute();

    }

    public void FireWait(RequestTaskFire fire)
    {

    }

    public ITaskRunner SetOptions(TaskRunnerOptions options) => null;
}

public class TaskRunner<T> : TaskRunnerBase<Action<T>>
{
    private T Parameter1 { get; set; }

    public TaskRunner(string name, Action<T> execute, TaskRunnerOptions options, T parameter)
        : base(name, execute, options)
    {
        this.Parameter1 = parameter;
    }

    public void Fire(RequestTaskFire fire,  T? parameter1, int count)
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



public class TaskRunner<T1,T2> : TaskRunnerBase<Action<T1,T2>>
{
    private T1 Parameter1 { get; set; }
    private T2 Parameter2 { get; set; }
    public TaskRunner(string name, Action<T1,T2> execute, TaskRunnerOptions options, T1 parameter1, T2 parameter2)
        : base(name, execute, options)
    {
        this.Parameter1 = parameter1;
        this.Parameter2 = parameter2;

    }

    public void Fire(RequestTaskFire fire,  T1? parameter1, int count)
    {
        if (parameter1 != null)
        {
            this.Parameter1 = parameter1;
        }

        base.Fire(fire, count);
    }


    public void Fire(RequestTaskFire fire,  T1? parameter1, T2? parameter2, int count)
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
        Execute(Parameter1,Parameter2);
        await Task.CompletedTask;
    }


}


public class TaskRunnerResult<TResult> : TaskRunnerBase<Func<TResult>>
{
    private readonly ConcurrentBag<TResult> _results = new ConcurrentBag<TResult>();
    //private SemaphoreSlim? semaphore;

    public TaskRunnerResult(string name, Func<TResult> execute, TaskRunnerOptions options) : base(name, execute, options)
    {
        // Initialize the semaphore with maxParallel if specified, otherwise no limit.

    }

    protected override async Task ExecuteTaskAsync(CancellationToken token)
    {
        // Respect cancellation
        token.ThrowIfCancellationRequested();

        var result = Execute();
        _results.Add(result);
        await Task.CompletedTask;
    }

}
