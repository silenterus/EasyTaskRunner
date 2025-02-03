namespace EasyTaskRunner;

using System.Collections.Concurrent;
using System.Net.Mime;
using Core;
using Data.Enums;
using Data.Events;
using Data.Interfaces;
using Data.Utilities;

public sealed class TaskRunnerService
{


    private static readonly Lazy<TaskRunnerService> _instance = new Lazy<TaskRunnerService>(() => new TaskRunnerService());

    public static TaskRunnerService Instance => _instance.Value;


    private readonly TaskRunnerManager _taskManager = new();

    private event EventHandler<TaskEventArgs>? TaskRegistered
    {
        add => _taskManager.TaskRegistered += value;
        remove => _taskManager.TaskRegistered -= value;
    }

    private event EventHandler<TaskEventArgs>? TaskUnregistered
    {
        add => _taskManager.TaskUnregistered += value;
        remove => _taskManager.TaskUnregistered -= value;
    }

    private event EventHandler<TaskEventArgs>? ResultAdded
    {
        add => _taskManager.ResultAdded += value;
        remove => _taskManager.ResultAdded -= value;
    }

    private event EventHandler<TaskEventArgs>? ParameterChanged
    {
        add => _taskManager.ParameterChanged += value;
        remove => _taskManager.ParameterChanged -= value;
    }

    private event EventHandler<TaskEventArgs>? OnSucceed
    {
        add => _taskManager.OnSucceed += value;
        remove => _taskManager.OnSucceed -= value;
    }

    private event EventHandler<TaskEventArgs>? OnError
    {
        add => _taskManager.OnError += value;
        remove => _taskManager.OnError -= value;
    }

    private event EventHandler<TaskEventArgs>? OnAbort
    {
        add => _taskManager.OnAbort += value;
        remove => _taskManager.OnAbort -= value;
    }

    private bool RegisterTask(string name, ITaskRunner runner)
    {
        return _taskManager.RegisterTask(name, runner);
    }

    private bool UnregisterTask(string name)
    {
        return _taskManager.UnregisterTask(name);
    }

    private bool RemoveRunner(string name)
    {
        return _taskManager.Remove(name);
    }

    private ITaskRunner? GetRunner(string name)
    {
        return _taskManager.Get(name);
    }

    private void SetParam<T>(string name, T parameter)
    {
        _taskManager.SetParam(name, parameter);
    }

    private void SetParam<T1, T2>(string name, T1 parameter1, T2 parameter2)
    {
        _taskManager.SetParam(name, parameter1, parameter2);
    }

    private void SetParam<T>(string name, Func<T> parameterProvider)
    {
        _taskManager.SetParam(name, parameterProvider);
    }

    private void SetParam<T1, T2>(string name, Func<T1> parameterProvider1, Func<T2> parameterProvider2)
    {
        _taskManager.SetParam(name, parameterProvider1, parameterProvider2);
    }

    private void SetOptions(string name, TaskRunnerOptions options)
    {
        _taskManager.SetOptions(name, options);
    }

    private void GetLogs(string name)
    {
        _taskManager.GetLogs(name);
    }

    private void Start(string name)
    {
        _taskManager.Start(name);
    }

    private void Stop(string name)
    {
        _taskManager.Stop(name);
    }

    private void Pause(string name)
    {
        _taskManager.Pause(name);
    }

    private void Resume(string name)
    {
        _taskManager.Resume(name);
    }

    private void StartAll()
    {
        _taskManager.StartAll();
    }

    private void StopAll()
    {
        _taskManager.StopAll();
    }

    private void PauseAll()
    {
        _taskManager.PauseAll();
    }

    private void ResumeAll()
    {
        _taskManager.ResumeAll();
    }

    private string GetStatusAll()
    {
        return _taskManager.GetStatusAll();
    }

    private string GetStatus(string name)
    {
        return _taskManager.GetStatus(name);
    }



    private IEnumerable<TResult> GetResults<TResult>(string name)
    {
        return _taskManager.GetResults<TResult>(name);
    }


    private void ClearResults(string name)
    {
        _taskManager.ClearResults(name);
    }

    private void ClearAll()
    {
        _taskManager.ClearAll();
    }

    private void Fire(string name, TaskFire fire)
    {
        _taskManager.Fire(name, fire);
    }

    private void Fire(string name, TaskFire fire, TaskRunnerOptions options)
    {
        _taskManager.Fire(name, fire, options);
    }

    private void Fire(string name, TaskFire fire, int count)
    {
        _taskManager.Fire(name, fire, count);
    }

    private void Fire(string name, TaskFire fire, int count, int maxParallel)
    {
        _taskManager.Fire(name, fire, count, maxParallel);
    }

    private void Fire(string name, TaskFire fire, int count, int maxParallel, int maxParallelCount)
    {
        _taskManager.Fire(name, fire, count, maxParallel, maxParallelCount);
    }

    private string Fire(string name, TaskFire fire, int count, params object[]? parameters)
    {
        return _taskManager.Fire(name, fire, count, parameters);
    }


    private void AddRunner(string name, Action execute)
    {
        _taskManager.Add(name, execute, new TaskRunnerOptions());
    }

    private void AddRunner(string name, Action execute, TaskRunnerOptions options)
    {
        _taskManager.Add(name, execute, options);
    }

    private void AddRunner<T>(string name, Action<T> execute, T parameter = default, TaskRunnerOptions? options = null)
    {
        _taskManager.Add(name, execute, parameter, options ?? new TaskRunnerOptions());
    }

    private void AddRunner<T1, T2>(string name, Action<T1, T2> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
    {
        _taskManager.Add(name, execute, parameter1, parameter2, options ?? new TaskRunnerOptions());
    }

    private void AddRunner<TResult>(string name, Func<TResult> execute, TaskRunnerOptions? options = null)
    {
        _taskManager.Add(name, execute, options ?? new TaskRunnerOptions());
    }

    private void AddRunner<T, TResult>(string name, Func<T, TResult> execute, T parameter, TaskRunnerOptions? options = null)
    {
        _taskManager.Add(name, execute, parameter, options ?? new TaskRunnerOptions());
    }

    private void AddRunner<T1, T2, TResult>(string name, Func<T1, T2, TResult> execute, T1 parameter1, T2 parameter2, TaskRunnerOptions? options = null)
    {
        _taskManager.Add(name, execute, parameter1, parameter2, options ?? new TaskRunnerOptions());
    }

    private void AddRunner(string name, ITaskRunner runner)
    {
        _taskManager.Add(name, runner);
    }


}
