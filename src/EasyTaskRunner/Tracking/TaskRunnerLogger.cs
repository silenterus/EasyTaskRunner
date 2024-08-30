namespace EasyTaskRunner.Tracking;

using System.Collections.Concurrent;

public enum TaskRunnerLogType
{
    Message = 1,
    Error = 2,
    Warning = 3,
    Info = 4,
    Exception = 5,
    Trace = 6,
    Critical = 7,
    Return = 8,
}

public interface ITaskRunnerLog
{
    void LogMessage(string message);
    void LogError(string error);
    IEnumerable<TaskRunnerLogEntry> GetAllEntries();
}

public class TaskRunnerLoggerBase
{
    private readonly ConcurrentDictionary<string, ITaskRunnerLog?> _logs = new ConcurrentDictionary<string, ITaskRunnerLog?>();

    public bool AddLog(string taskId, ITaskRunnerLog? log)
    {
        return _logs.TryAdd(taskId, log);
    }

    public ITaskRunnerLog? GetLog(string taskId)
    {
        _logs.TryGetValue(taskId, out ITaskRunnerLog? log);
        return log;
    }
}

public class TaskRunnerLog : ITaskRunnerLog
{
    private readonly ConcurrentQueue<TaskRunnerLogEntry> _entries = new ConcurrentQueue<TaskRunnerLogEntry>();

    public void LogMessage(string message)
    {
        _entries.Enqueue(new TaskRunnerLogEntry("INFO", message));
    }

    public void LogError(string error)
    {
        _entries.Enqueue(new TaskRunnerLogEntry("ERROR", error));
    }

    public IEnumerable<TaskRunnerLogEntry> GetAllEntries()
    {
        return _entries;
    }
}

public class TaskRunnerLogEntry(string messageType, string message)
{
    public DateTime Timestamp { get; private set; } = DateTime.Now;
    public string MessageType { get; private set; } = messageType;
    public string Message { get; private set; } = message;

    public override string ToString()
    {
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{MessageType}] {Message}";
    }
}
