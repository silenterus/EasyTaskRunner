namespace EasyTaskRunner.Data.Interfaces;

public interface ITaskRunnerWithResult<TResult> : ITaskRunner
{
    IEnumerable<TResult> GetResults();
    void ClearResults();
}
