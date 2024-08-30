namespace EasyTaskRunner.Data.Utilities;

public class ErrorValue
{
    private int MaxError { get; set; } = 0;
    private int ErrorCount { get; set; } = 0;

    public ErrorValue(int maxError)
    {
        MaxError = maxError;
    }

    public void IncrementError()
    {
        ErrorCount++;
    }

    public void Reset()
    {
        ErrorCount = 0;
    }

    public bool IsLimit()
    {
        return MaxError > 0 && ErrorCount >= MaxError;
    }
}
