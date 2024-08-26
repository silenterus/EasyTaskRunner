﻿using EasyTaskRunner.Data.Enums;
namespace omni__penetration.Tracking;

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


    public TaskRunnerLogMessage(TaskRunnerLogType logType,RequestTaskStatus status, string message, Exception? exception = null)
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



public class TaskRunnerLogFormatter : ITaskRunnerLogFormatter
{
    public string Format(TaskRunnerLogMessage logMessage)
    {
        var logPrefix = $"[{logMessage.Timestamp:u}] [{logMessage.LogType}]";
        var logContent = logMessage.Message;

        if (logMessage.Exception != null)
        {
            logContent += $" Exception: {logMessage.Exception.Message}";
        }

        return $"{logPrefix}: {logContent}";
    }
}

public class TaskRunnerRequestLogHandler : ITaskRunnerLogHandler
{

    private readonly ITaskRunnerLogFormatter _formatter;
    public TaskRunnerRequestLogHandler(ITaskRunnerLogFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Handle(TaskRunnerLogMessage logMessage)
    {
        var formattedMessage = _formatter.Format(logMessage);
        Console.WriteLine(formattedMessage);
    }
}