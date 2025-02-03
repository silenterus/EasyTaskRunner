namespace EasyTaskRunner.Data.Interfaces;

using Enums;
public interface ITaskRunnerParam : ITaskRunner
{
    void Fire(TaskFire fire, int count, params object[]? parameters);
}

