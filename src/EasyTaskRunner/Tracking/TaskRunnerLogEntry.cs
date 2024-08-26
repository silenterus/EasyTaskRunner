namespace omni__penetration.Tracking;

public class TaskRunnerLogEntry2
{
    public DateTime Timestamp { get; private set; }
    public string MessageType { get; private set; }
    public string Message { get; private set; }

    public TaskRunnerLogEntry2(string messageType, string message)
    {
        Timestamp = DateTime.Now; // Captures the current time when the log entry is created
        MessageType = messageType;
        Message = message;
    }

    public override string ToString()
    {
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{MessageType}] {Message}";
    }
}
