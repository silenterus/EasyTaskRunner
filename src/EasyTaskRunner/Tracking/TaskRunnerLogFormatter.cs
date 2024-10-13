namespace EasyTaskRunner.Tracking;

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

