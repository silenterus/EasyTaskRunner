using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Utilities;

namespace EasyTaskRunner.Data.Interfaces
{
    using TaskStatus = Enums.TaskStatus;
    public interface ITaskRunner
    {
        void Fire(TaskFire fire);
        void Fire(TaskFire fire, TaskRunnerOptions options);
        void Fire(TaskFire fire, int count);
        void Fire(TaskFire fire, int count, int maxParallel);
        void Fire(TaskFire fire, int count, int maxParallel, int maxParallelCount);
        string Status();


        TaskStatus GetTaskStatus();
        ITaskRunner SetOptions(TaskRunnerOptions options);
        void GetLogs();
    }
}
