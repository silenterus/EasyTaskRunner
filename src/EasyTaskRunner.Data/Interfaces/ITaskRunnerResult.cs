namespace EasyTaskRunner.Data.Interfaces;

public interface ITaskRunnerResult<out TResult> : ITaskRunner
{
    IEnumerable<TResult> GetResults();
    void ClearResults();
}
