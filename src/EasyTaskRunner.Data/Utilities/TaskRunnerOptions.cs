namespace EasyTaskRunner.Data.Utilities;

public class TaskRunnerOptions(int count = 1, int maxParallel = 0, int maxCount = 1, bool useSemaphore = false, bool endless = false, int delay = 1000, bool useLog = false)
{
    public int Count { get; private set; } = count;
    public int Delay { get; private set; } = delay;
    public bool Endless { get; private set; } = endless;
    public bool AsyncRunners { get; private set; } = false;
    public ErrorCounter ErrorCount { get; private set; } = new ErrorCounter();
    public int MaxParallel { get; private set; } = maxParallel;

    public int Cycles { get; private set; } = maxCount;

    public bool UseLog { get; private set; } = useLog;

 

    public TaskRunnerOptions SetEndless(bool endless)
    {
        Endless = endless;
        return this;
    }

    public TaskRunnerOptions SetLog(bool activate)
    {
        UseLog = activate;
        return this;
    }



    public TaskRunnerOptions SetErrors((string name, int maxError)[] errors)
    {
        ErrorCount = new ErrorCounter(errors);
        return this;
    }

    public TaskRunnerOptions SetCounts(int count, int maxParallel, int maxCount)
    {
        return SetCount(count: count).SetMaxParallel(maxParallel: maxParallel).SetMaxCount(maxCount: maxCount);
    }

    public TaskRunnerOptions SetCount(int count)
    {
        if (count > 0)
        {
            Count = count;
        }

        if (Count < 1)
        {
            Count = 1;
        }
        return this;
    }

    private TaskRunnerOptions SetMaxCount(int maxCount)
    {
        if (maxCount > 0)
        {
            Cycles = maxCount;
        }
        else
        {
            Cycles = 1;
        }

        if (Cycles < 1)
        {
            Cycles = 1;
        }
        return this;
    }

    public TaskRunnerOptions SetMaxParallel(int maxParallel)
    {
        if (maxParallel > 1)
        {
            MaxParallel = maxParallel;
        }
        else
        {
            MaxParallel = 0;
        }
        return this;
    }

    public void Validate()
    {
        // Ensure that Count is at least 1
        if (Count < 1)
        {
            Count = 1;
        }

        if (Cycles < 1)
        {
            Cycles = 1;
        }

        if (MaxParallel < 0)
        {
            MaxParallel = 0;
        }

        if (MaxParallel > 0 && Cycles == 0)
        {
            Cycles = 1;
        }

        if (MaxParallel > 0 && MaxParallel < Cycles)
        {
            MaxParallel = Cycles;
        }

        if (Delay < 0)
        {
            Delay = 0;
        }
    }

    public void CopyFrom(TaskRunnerOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), "Provided TaskRunnerOptions is null");
        }

        Count = options.Count;
        Delay = options.Delay;
        Endless = options.Endless;
        AsyncRunners = options.AsyncRunners;
        ErrorCount = new ErrorCounter();
        MaxParallel = options.MaxParallel;
        Cycles = options.Cycles;
        UseLog = options.UseLog;
    }


}
