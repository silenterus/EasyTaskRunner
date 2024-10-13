namespace EasyTaskRunner.Tracking;

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
