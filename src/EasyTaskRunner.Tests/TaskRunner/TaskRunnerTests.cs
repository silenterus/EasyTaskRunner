using EasyTaskRunner.Data.Enums;
using EasyTaskRunner.Extensions.Helper;
using Xunit;
using Xunit.Abstractions;
namespace EasyTaskRunner.Tests.TaskRunner
{


    public class TaskRunnerTests
    {

        private bool _logActive = false;

        private int _executionCount;
        private readonly ITestOutputHelper _testOutputHelper;

        public TaskRunnerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private void Log(string msg,int count)
        {
           if(_logActive)return;
           _testOutputHelper.WriteLine($"{msg} - executionCount:[{_executionCount}] count:[{count}]");
        }

        private void Log(RequestTaskFire status,int count,string msg = "")
        {
            if(!_logActive)return;
            _testOutputHelper.WriteLine($"{status.ToString()} {msg} - executionCount:[{_executionCount}] count:[{count}]");
        }
        private void SampleExecution()
        {
            _executionCount++;
        }

        [Fact]
        public async Task ConcurrentFire_CallsDoNotInterfere()
        {
            var name = "ConcurrentFireTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 10, maxParallel: 0, endless: false, delay: 0);
            var taskRunner1 = new Core.TaskRunner(name, SampleExecution, options);
           // var taskRunner2 = new EasyTaskRunner.Core.TaskRunner(name, SampleExecution, options);

            var fireTask1 = Task.Run(() => taskRunner1.Fire(RequestTaskFire.Start));
            await Task.Yield();
            var fireTask2 = Task.Run(() => taskRunner1.Fire(RequestTaskFire.Start));

            await Task.WhenAll(fireTask1, fireTask2);
            await Task.Delay(2000);

            Assert.Equal(20, _executionCount);
            Assert.Contains("completed", taskRunner1.Status());
            //Assert.Contains("completed", taskRunner2.Status());

        }

        [Fact]
        public async Task StartAsync_RunsTaskSpecifiedTimes()
        {
            var name = "TestTask";
            var count = 3;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);

            Assert.Equal(count, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public async Task StartAsync_AlreadyStartedTest()
        {
            var name = "TestTask";
            var count = 3;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 500);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(100);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));

            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task StopAsync_StopsTaskBeforeCompletion()
        {
            var name = "TestTask";
            var count = 5;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 1000);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Delay(500);

            Assert.True(_executionCount < count);
            Assert.Contains("stopped", taskRunner.Status());
        }

        [Fact]
        public async Task RestartAsync_RestartsTaskProperly()
        {
            var name = "TestTask";
            var count = 3;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 10);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Restart));
            await Task.Delay(500);

            Assert.Equal(count * 2, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public async Task FireWait_SynchronouslyExecutesOnce()
        {
            var name = "TestTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: 0, endless: false, delay: 0);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);
            taskRunner.Fire(RequestTaskFire.FireWait);
            var status = taskRunner.Status();
            await Task.Delay(500);

            Assert.Equal(1, _executionCount);
            Assert.Contains("completed execution", status);
        }

        [Fact]
        public async Task EndlessExecution_RunsIndefinitelyUntilStopped()
        {
            var name = "EndlessTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: 0, endless: true, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Delay(500);

            Assert.True(_executionCount > 1);
            Assert.Contains("stopped", taskRunner.Status());
        }

        [Fact]
        public async Task Fire_StartsTaskOnceEvenWithCountGreaterThanOne()
        {
            var name = "FireTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 5, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Fire));
            await Task.Delay(500);

            Assert.Equal(1, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public void InvalidCount_SetsDefaultCount()
        {
            var name = "InvalidCountTask";
            var options = TaskRunnerHelper.CreateOptions(count: 0, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            Assert.Contains("[0/1]", taskRunner.Status());
        }

        [Fact]
        public async Task FaultedExecution_ChangesStatusToFaulted()
        {
            var name = "FaultedTask";
            void FaultyExecution() => throw new Exception("Test Exception");
            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, FaultyExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(200);

            Assert.Contains("faulted", taskRunner.Status());
        }


        [Fact]
        public async Task EndlessExecution_PauseAndStop()
        {
            var name = "EndlessPauseStopTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: 0, endless: true, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(200);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Pause));
            await Task.Delay(200);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Delay(200);

            Assert.True(_executionCount > 1);
            Assert.Contains("stopped", taskRunner.Status());
        }

        [Fact]
        public async Task ToggleAsync_ResumesExecution()
        {
            var name = "ToggleAsync";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 5, maxParallel: 0, endless: false, delay: 200);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            Log(RequestTaskFire.Start,count:options.Count);
            await Task.Delay(100);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Toggle));
            Log(RequestTaskFire.Pause,count:options.Count,"Before");
            await Task.Delay(1000);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Toggle));
            Log(RequestTaskFire.Pause,count:options.Count);
            await Task.Delay(80);
            Log(RequestTaskFire.Pause,count:options.Count,"Delay");


            Assert.True(_executionCount > 1);
            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task UnPauseAsync_ResumesExecution()
        {
            var name = "UnpauseTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 5, maxParallel: 0, endless: false, delay: 200);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            //this._testOutputHelper.WriteLine($"Start executionCount:[{_executionCount}] count:[{5}]");
            await Task.Delay(100);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Pause));
            //this._testOutputHelper.WriteLine($"Pause Before executionCount:[{_executionCount}] count:[{5}]");

            await Task.Delay(1000);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.UnPause));
            //this._testOutputHelper.WriteLine($"Pause executionCount:[{_executionCount}] count:[{5}]");
            await Task.Delay(80);
            //this._testOutputHelper.WriteLine($"Pause Delay executionCount:[{_executionCount}] count:[{5}]");


            Assert.True(_executionCount > 1);
            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task MultipleStartCalls_DoesNotRestartTask()
        {
            var name = "MultipleStartTask";
            var count = 5;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(100);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);

            Assert.Equal(count, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public async Task StopExecution_ChangesStatusToStop()
        {
            var name = "CancelTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 3, maxParallel: 0, endless: false, delay: 1000);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Delay(50);

            Assert.Contains("stopped", taskRunner.Status());
        }

        [Fact]
        public async Task RestartAsync_ContinuesFromLastState()
        {
            var name = "RestartContinuationTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 3, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(200);
            var countBeforeRestart = _executionCount;
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Restart));
            await Task.Delay(140);

            Assert.True(_executionCount > countBeforeRestart);
            Assert.Contains("running", taskRunner.Status());
        }


        [Fact]
        public async Task FireParallelEndless_RunsTasksConcurrentlyUntilStopped()
        {
            var name = "ParallelEndlessTask";
            _executionCount = 0;

            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: 3, endless: true, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Delay(500);

            Assert.True(_executionCount > 3);
            Assert.Contains("stopped", taskRunner.Status());
        }

        [Fact]
        public async Task FireParallel_RunsTasksInParallelOnce()
        {
            _executionCount = 0;
            var name = "ParallelFireTask";
            var maxParallel = 4;
            var delay = 50;
            var options = TaskRunnerHelper.CreateOptions(count: 1, maxParallel: maxParallel, endless: false, delay: delay);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(delay*2);

            Assert.Equal(maxParallel, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public async Task ExecutionWithSemaphore_LimitsConcurrency()
        {
            var name = "SemaphoreTask";
            var count = 10;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 2, endless: false, delay: 100, useSemaphore: true);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(1500);

            Assert.Equal(count, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }

        [Fact]
        public void TaskCanStart_ReturnsFalseIfTaskAlreadyRunning()
        {
            var name = "TaskStartCheck";
            var options = TaskRunnerHelper.CreateOptions(count: 3, maxParallel: 0, endless: false, delay: 100);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            taskRunner.Fire(RequestTaskFire.Start);
            taskRunner.Fire(RequestTaskFire.Start);

            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task ParallelExecution_RunsTasksConcurrently()
        {
            var name = "ParallelTask";
            var count = 2;
            var maxCount = 10;
            var maxParallel = 3;
            var delay = 100;
            var allDelay = 500;
            var expectedCount = count*maxCount;
            _executionCount = 0;

            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: maxParallel, maxCount:maxCount, useSemaphore:true, endless:false,delay: delay);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(allDelay);

            Assert.Equal(expectedCount, _executionCount);
            Assert.Contains("completed execution", taskRunner.Status());
        }
        [Fact]
        public async Task PauseAsync_PausesExecution()
        {
            var name = "PauseTask";
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: 3, maxParallel: 0, maxCount:0, useSemaphore:false, endless:false,delay: 1000);

            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(500);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Pause));

            var previousCount = _executionCount;
            await Task.Delay(2000);

            Assert.Equal(previousCount, _executionCount);
            Assert.Contains("paused", taskRunner.Status());
        }
        [Fact]
        public async Task StartStopAndRestart()
        {
            // Arrange
            var name = "PauseTask";
            var count = 5;
            _executionCount = 0;
            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0, endless: false, delay: 20);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            // Act
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Start));
            await Task.Delay(200);
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Stop));
            await Task.Run(() => taskRunner.Fire(RequestTaskFire.Restart));
            await Task.Delay(80); // Allow time for some executions
            // Assert
            Assert.True(_executionCount > count);
            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task PauseAndUnpause_ResumesTaskAfterPause()
        {
            var name = "PauseTask";
            var count = 15;
            _executionCount = 0;

            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0,maxCount:0, endless: false, delay: 200);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            taskRunner.Fire(RequestTaskFire.Start);
            await Task.Delay(100);
            taskRunner.Fire(RequestTaskFire.Pause);

            await Task.Delay(2500);
            taskRunner.Fire(RequestTaskFire.UnPause);
            await Task.Delay(80);



            Assert.True(_executionCount < count);
            Assert.Contains("running", taskRunner.Status());
        }

        [Fact]
        public async Task ToggleAndToggle_ResumesTaskAfterPause()
        {
            var name = "ToggleTask";
            var count = 15;
            _executionCount = 0;

            var options = TaskRunnerHelper.CreateOptions(count: count, maxParallel: 0,maxCount:0, endless: false, delay: 200);
            var taskRunner = new Core.TaskRunner(name, SampleExecution, options);

            taskRunner.Fire(RequestTaskFire.Start);
            await Task.Delay(100);
            taskRunner.Fire(RequestTaskFire.Toggle);

            await Task.Delay(2500);
            taskRunner.Fire(RequestTaskFire.Toggle);
            await Task.Delay(80);
            await Task.Yield();

            Assert.True(_executionCount < count);
            Assert.Contains("running", taskRunner.Status());
        }

    }



}
