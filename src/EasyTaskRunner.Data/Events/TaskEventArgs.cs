namespace EasyTaskRunner.Data.Events;

using Interfaces;
public class TaskEventArgs(string taskName, ITaskRunner taskRunner) : EventArgs
{
    public string TaskName { get; } = taskName;
    public ITaskRunner TaskRunner { get; } = taskRunner;

}
