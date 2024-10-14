namespace EasyTaskRunner.Data.Interfaces;

using Enums;
public interface ITaskRunnerWithParam : ITaskRunner
{
    void Fire(RequestTaskFire fire, int count, params object[]? parameters);
}

