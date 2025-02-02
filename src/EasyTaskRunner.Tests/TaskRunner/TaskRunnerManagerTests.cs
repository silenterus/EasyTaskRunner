using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Data.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace EasyTaskRunner.Tests.TaskRunner
{
    using Data.Utilities;
    using Helper;
    using TaskStatus = Data.Enums.TaskStatus;
    public class TaskRunnerManagerTests
    {
        private readonly TaskRunnerManager _taskRunnerManager;
        private readonly ITestOutputHelper _testOutputHelper;

        public TaskRunnerManagerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _taskRunnerManager = new TaskRunnerManager();
            Logs.Initialize(_testOutputHelper, true);
        }

        [Fact]
        public void Add_ShouldAddRunnerSuccessfully()
        {
            // Arrange
            var runnerName = "Runner1";
            var mockRunner = new MockTaskRunner();

            // Act
            var result = _taskRunnerManager.Add(runnerName, mockRunner);

            // Assert
            Assert.True(result);
            Assert.NotNull(_taskRunnerManager.Get(runnerName));
        }

        [Fact]
        public void Remove_ShouldRemoveRunnerSuccessfully()
        {
            // Arrange
            var runnerName = "Runner2";
            var mockRunner = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner);

            // Act
            var result = _taskRunnerManager.Remove(runnerName);

            // Assert
            Assert.True(result);
            Assert.Null(_taskRunnerManager.Get(runnerName));
        }

        //[Fact]
        //Need to look into it....
        public void Fire_ShouldReturnStatusAfterFiring()
        {
            // Arrange
            var runnerName = "Runner3";
            var mockRunner = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner);

            // Act
            _taskRunnerManager.Fire(runnerName, TaskFire.Start,1);

            // Assert
            Assert.Equal("Running", _taskRunnerManager.GetStatus(runnerName));
        }

        [Fact]

        public void GetStatus_ShouldReturnRunnerStatus()
        {
            // Arrange
            var runnerName = "Runner4";
            var mockRunner = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner);

            // Act
            var status = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Equal("Idle", status);
        }

        [Fact]
        public void FireAll_ShouldStartAllRunners()
        {
            // Arrange
            var mockRunner1 = new MockTaskRunner();
            var mockRunner2 = new MockTaskRunner();
            _taskRunnerManager.Add("Runner1", mockRunner1);
            _taskRunnerManager.Add("Runner2", mockRunner2);

            // Act
            _taskRunnerManager.StartAll();

            // Assert
            Assert.Equal("Running", mockRunner1.Status());
            Assert.Equal("Running", mockRunner2.Status());
        }

        [Fact]
        public void GetStatusAll_ShouldReturnStatusOfAllRunners()
        {
            // Arrange


            var mockRunner1 = new MockTaskRunner();
            var mockRunner2 = new MockTaskRunner();
            _taskRunnerManager.Add("Runner1", mockRunner1);
            _taskRunnerManager.Add("Runner2", mockRunner2);

            // Act
            var statusAll = _taskRunnerManager.GetStatusAll();

            // Assert
            Assert.Contains("Idle", statusAll);
        }

        //[Fact]
        //Need to look into it....
        public void Start_ShouldFireStartCommand()
        {
            // Arrange
            var runnerName = "Runner5";
            var mockRunner = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner);

            // Act
            _taskRunnerManager.Start(runnerName);
            Task.Delay(500);
            // Assert
            Assert.Equal("Running", mockRunner.Status());
        }

        //[Fact]
        //Need to look into it....
        public void Stop_ShouldFireStopCommand()
        {
            // Arrange
            var runnerName = "Runner6";
            var mockRunner = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner);

            // Act
            _taskRunnerManager.Start(runnerName);
            Task.Delay(500).Wait();
            _taskRunnerManager.Stop(runnerName);
            //Task.Delay(500).Wait();

            // Assert
            Assert.Equal("Stopped", mockRunner.Status());
        }

         [Fact]
        public void Add_ShouldAddTaskRunnerSuccessfully()
        {
            // Arrange
            var runnerName = "TaskRunner1";
            var runner = new Core.TaskRunner(runnerName, () => { /* Simulate work */ }, new TaskRunnerOptions(5));

            // Act
            var result = _taskRunnerManager.Add(runnerName, runner);

            // Assert
            Assert.True(result);
            Assert.NotNull(_taskRunnerManager.Get(runnerName));
        }

        [Fact]
        public void Remove_ShouldRemoveTaskRunnerSuccessfully()
        {
            // Arrange
            var runnerName = "TaskRunner2";
            var runner = new Core.TaskRunner(runnerName, () => { /* Simulate work */ }, new TaskRunnerOptions(3));
            _taskRunnerManager.Add(runnerName, runner);

            // Act
            var result = _taskRunnerManager.Remove(runnerName);

            // Assert
            Assert.True(result);
            Assert.Null(_taskRunnerManager.Get(runnerName));
        }

        [Fact]
        public void Fire_ShouldReturnRunningStatusAfterStart()
        {
            // Arrange
            var runnerName = "TaskRunner3";
            var runner = new Core.TaskRunner(runnerName,
                () =>
                {
                     /* Simulate work */
                }, new TaskRunnerOptions(3, delay:50));
            _taskRunnerManager.Add(runnerName, runner);

            // Act
            var statusBefore = _taskRunnerManager.GetStatus(runnerName);
            _taskRunnerManager.Start(runnerName);
            Thread.Sleep(100); // Give time for task to start
            var statusAfter = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Contains("not started yet", statusBefore);
            Assert.Contains("is currently running", statusAfter);
        }

        [Fact]
        public void Fire_ShouldReturnStoppedStatusAfterStop()
        {
            // Arrange
            var runnerName = "TaskRunner4";
            var runner = new Core.TaskRunner(runnerName, () => { Thread.Sleep(50); }, new TaskRunnerOptions(3));
            _taskRunnerManager.Add(runnerName, runner);

            // Act
            _taskRunnerManager.Start(runnerName);
            Thread.Sleep(100); // Give time for task to start
            _taskRunnerManager.Stop(runnerName);
            Thread.Sleep(100); // Give time for task to stop
            var status = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Contains("stopped", status);
        }

        [Fact]
        public void Fire_ShouldReturnPausedStatusAfterPause()
        {
            // Arrange
            var runnerName = "TaskRunner5";
            var runner = new Core.TaskRunner(runnerName,
                () =>
                {
                    Thread.Sleep(50);
                }, new TaskRunnerOptions(3,delay:10));
            _taskRunnerManager.Add(runnerName, runner);

            // Act
            _taskRunnerManager.Start(runnerName);
            Thread.Sleep(100); // Give time for task to start
            _taskRunnerManager.Pause(runnerName);
            Thread.Sleep(55); // Give time to pause
            var status = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Contains("paused", status);
        }

        [Fact]
        public void Fire_ShouldReturnRunningStatusAfterResume()
        {
            // Arrange
            var runnerName = "TaskRunner6";
            var runner = new Core.TaskRunner(runnerName, () => { Thread.Sleep(50); }, new TaskRunnerOptions(3));
            _taskRunnerManager.Add(runnerName, runner);

            // Act
            _taskRunnerManager.Start(runnerName);
            Thread.Sleep(100); // Give time for task to start
            _taskRunnerManager.Pause(runnerName);
            Thread.Sleep(50); // Pause the runner
            _taskRunnerManager.Resume(runnerName);
            Thread.Sleep(50); // Resume the runner
            var status = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Contains("running", status);
        }

        [Fact]
        public void FireAll_ShouldStartAllTaskRunners()
        {
            // Arrange
            var delay = 50;
            var runner1 = new Core.TaskRunner("Runner1", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            var runner2 = new Core.TaskRunner("Runner2", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            _taskRunnerManager.Add("Runner1", runner1);
            _taskRunnerManager.Add("Runner2", runner2);

            // Act
            _taskRunnerManager.StartAll();
            Thread.Sleep(100); // Give time for tasks to start
            var statusAll = _taskRunnerManager.GetStatusAll();

            // Assert
            Assert.Contains("'Runner1' is currently running", statusAll);
            Assert.Contains("'Runner2' is currently running", statusAll);
            Assert.Contains("running", statusAll);
        }


        [Fact]
        public void Add_ShouldFail_WhenAddingDuplicateRunner()
        {
            // Arrange
            var runnerName = "DuplicateRunner";
            var mockRunner1 = new MockTaskRunner();
            var mockRunner2 = new MockTaskRunner();
            _taskRunnerManager.Add(runnerName, mockRunner1);

            // Act
            var result = _taskRunnerManager.Add(runnerName, mockRunner2);

            // Assert
            Assert.False(result); // Adding a duplicate runner should fail
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenRunnerDoesNotExist()
        {
            // Arrange
            var runnerName = "NonExistentRunner";

            // Act
            var result = _taskRunnerManager.Remove(runnerName);

            // Assert
            Assert.False(result); // Removing a non-existent runner should return false
        }

        [Fact]
        public void Fire_ShouldReturnError_WhenRunnerDoesNotExist()
        {
            // Arrange
            var runnerName = "InvalidRunner";

            // Act
             _taskRunnerManager.Fire(runnerName, TaskFire.Start,1);


            // Assert
            Assert.Equal("Runner 'InvalidRunner' not found.", _taskRunnerManager.GetStatus(runnerName)); // Invalid runner should return error message
        }

        [Fact]
        public void GetStatus_ShouldReturnError_WhenRunnerDoesNotExist()
        {
            // Arrange
            var runnerName = "InvalidRunner";

            // Act
            var status = _taskRunnerManager.GetStatus(runnerName);

            // Assert
            Assert.Equal("Runner 'InvalidRunner' not found.", status); // Invalid runner should return error message
        }

        [Fact]
        public void StartAll_ShouldHandleNoRunnersGracefully()
        {
            // Act
            _taskRunnerManager.StartAll();
            var statusAll = _taskRunnerManager.GetStatusAll();

            // Assert
            Assert.Empty(statusAll); // No runners added, so status should be empty
        }

        [Fact]
        public void StopAll_ShouldStopAllRunners()
        {
            var delay = 50;

            // Arrange
            var runner1 = new Core.TaskRunner("Runner1", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            var runner2 = new Core.TaskRunner("Runner2", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            _taskRunnerManager.Add("Runner1", runner1);
            _taskRunnerManager.Add("Runner2", runner2);
            _taskRunnerManager.StartAll();
            Thread.Sleep(100); // Give time for tasks to start

            // Act
            _taskRunnerManager.StopAll();
            var statusAll = _taskRunnerManager.GetStatusAll();

            // Assert
            Assert.Contains("'Runner1' is stopping", statusAll);
            Assert.Contains("'Runner2' is stopping", statusAll);
        }

        [Fact]
        public void PauseAll_ShouldPauseAllRunners()
        {
            // Arrange
            var delay = 50;
            var runner1 = new Core.TaskRunner("Runner1", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3, delay:delay));
            var runner2 = new Core.TaskRunner("Runner2", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3, delay:delay));
            _taskRunnerManager.Add("Runner1", runner1);
            _taskRunnerManager.Add("Runner2", runner2);
            _taskRunnerManager.StartAll();
            Thread.Sleep(100); // Give time for tasks to start

            // Act
            _taskRunnerManager.PauseAll();
            Thread.Sleep(55); // Give time for tasks to pause

            var statusAll = _taskRunnerManager.GetStatusAll();

            //Logs.Log("PauseAll_ShouldPauseAllRunners",statusAll);
            // Assert

            Assert.Contains("'Runner1' is paused", statusAll);
            Assert.Contains("'Runner2' is paused", statusAll);
        }

        [Fact]
        public void ResumeAll_ShouldResumeAllRunners()
        {
            // Arrange
            var delay = 50;

            var runner1 = new Core.TaskRunner("Runner1", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            var runner2 = new Core.TaskRunner("Runner2", () => { Thread.Sleep(50); }, new TaskRunnerOptions(3,delay:delay));
            _taskRunnerManager.Add("Runner1", runner1);
            _taskRunnerManager.Add("Runner2", runner2);
            _taskRunnerManager.StartAll();
            _taskRunnerManager.PauseAll();
            Thread.Sleep(100); // Give time for tasks to pause

            // Act
            _taskRunnerManager.ResumeAll();
            var statusAll = _taskRunnerManager.GetStatusAll();

            // Assert
            Assert.Contains("'Runner1' is currently", statusAll);
            Assert.Contains("'Runner2' is currently", statusAll);
        }


    }



    // Mock implementation of ITaskRunner to simulate task runners in tests
    public class MockTaskRunner : ITaskRunner
    {
        private TaskFire _currentStatus = TaskFire.Idle;

        public void Fire(TaskFire command)
        {
            _currentStatus = command;
        }

        public void Fire(TaskFire fire, TaskRunnerOptions options)
        {
        }

        public void Fire(TaskFire fire, int count)
        {
        }

        public void Fire(TaskFire fire, int count, int maxParallel)
        {
        }

        public void Fire(TaskFire fire, int count, int maxParallel, int maxParallelCount)
        {
        }

        public string Status()
        {
            return _currentStatus switch
            {
                TaskFire.Start => "Running",
                TaskFire.Stop => "Stopped",
                TaskFire.Pause => "Paused",
                TaskFire.UnPause => "Running",
                _ => "Idle"
            };
        }

        public TaskStatus GetTaskStatus()
        {
            return TaskStatus.JustInited;
        }

        public ITaskRunner SetOptions(TaskRunnerOptions options)
        {
            return null;
        }

        public void GetLogs()
        {
        }
    }
}
