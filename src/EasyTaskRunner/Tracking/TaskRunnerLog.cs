namespace EasyTaskRunner.Tracking;

using EasyTaskRunner.Data.Enums;

public interface ITaskRunnerLogFormatter
{
    string Format(TaskRunnerLogMessage logMessage);
}

public interface ITaskRunnerLogHandler
{
    void Handle(TaskRunnerLogMessage logMessage);
}

public class TaskRunnerLogMessage
{
    public TaskRunnerLogType LogType { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }
    public Exception? Exception { get; }

    public RequestTaskStatus Status { get; }

    public TaskRunnerLogMessage(TaskRunnerLogType logType, RequestTaskStatus status, string message, Exception? exception = null)
    {
        LogType = logType;
        Message = message;
        Timestamp = DateTime.UtcNow;
        Exception = exception;
        Status = status;
    }
}

public class TaskRunnerLogger
{
    private readonly List<ITaskRunnerLogHandler> _handlers = new List<ITaskRunnerLogHandler>();

    public void AddHandler(ITaskRunnerLogHandler handler)
    {
        _handlers.Add(handler);
    }

    public void Log(TaskRunnerLogType logType, RequestTaskStatus status, string message, Exception? exception = null)
    {
        var logMessage = new TaskRunnerLogMessage(logType, status, message, exception);
        foreach (var handler in _handlers)
        {
            handler.Handle(logMessage);
        }
    }
}

