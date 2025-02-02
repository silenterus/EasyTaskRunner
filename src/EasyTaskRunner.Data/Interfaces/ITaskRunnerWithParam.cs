namespace EasyTaskRunner.Data.Interfaces;

using Enums;
public interface ITaskRunnerWithParam : ITaskRunner
{
    void Fire(TaskFire fire, int count, params object[]? parameters);
}

