using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Data.Interfaces
{
    public interface ITaskRunner
    {
        void Fire(RequestTaskFire fire);
        void Fire(RequestTaskFire fire, TaskRunnerOptions options);
        void Fire(RequestTaskFire fire, int count);
        void Fire(RequestTaskFire fire, int count, int maxParallel);
        void Fire(RequestTaskFire fire, int count, int maxParallel, int maxParallelCount);
        string Status();
        RequestTaskStatus GetTaskStatus();
        ITaskRunner SetOptions(TaskRunnerOptions options);
        void GetLogs();
    }


}
