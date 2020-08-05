using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Common.Helpers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch.Exceptions;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using Moq;
using ProcessMonitoring.Models;
using SharpPcap;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch
{
    public abstract class IProcessWatcherTest
    {
        protected const string INITIAL_TARGET = "chrome";
        
        protected bool CaptureDeviceOpen = false;
        protected bool JobTimerRunning = false;
        protected ElapsedEventHandler WatchJobHandler;
        protected string CaptureDeviceFilter = string.Empty;
        protected readonly List<int> OpenPorts = new List<int>(){1, 2, 3, 4};

        [Fact]
        public void GivenConfigFile_WhenGettingInitialProcessToWatch_ThenItReturnsProperProcessWatch()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();

            IEnumerable<IProcessWatch> initialTargets = watchdog.GetProcessUnderWatch();

            var processWatches = initialTargets as IProcessWatch[] ?? initialTargets.ToArray();
            Assert.Single(processWatches);
            Assert.Contains(INITIAL_TARGET, processWatches.Single().ProcessName);
        }
        
        [Fact]
        public void GivenRunningProcess_WhenAddProcessWatch_ProcessIsRunning()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();

            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            IProcessWatch processWatch =
                processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(runningProcess.ProcessName));

            Assert.True(processWatch?.IsRunning) ;
            Assert.Equal(2, processWatches.Count());
        }

        [Fact]
        public void GivenNotRunningProcess_WhenAddProcessWatch_ProcessIsNotRunning()
        {
            const string processName = "NOT_A_PROCESS";
            IProcessWatcher watchdog = GivenProcessWatcher();

            watchdog.AddProcessToWatchList(processName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            IProcessWatch processWatch =
                processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(processName));

            Assert.False(processWatch?.IsRunning) ;
        }

        [Fact]
        public void GivenWatchJob_WhenStartWatch_ThenItRegistersHandler()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();
            
            watchdog.StartCapture();

            Assert.True(JobTimerRunning);
            Assert.NotNull(WatchJobHandler);
        }

        [Fact]
        public void GivenWatchJob_WhenStopWatch_ThenItUnregistersHandler()
        {
            WatchJobHandler = null;
            JobTimerRunning = false;
            CaptureDeviceOpen = true;
            IProcessWatcher watchdog = GivenProcessWatcher();
            
            watchdog.StartCapture();
            watchdog.StopCapture();
            
            Assert.False(JobTimerRunning);
            Assert.Null(WatchJobHandler);
            Assert.False(CaptureDeviceOpen);
        }

        [Fact]
        public void GivenAddedProcessWatchWithNoCapture_WhenStartCapture_CaptureDeviceFilterDoesntChange()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            watchdog.StartCapture();
            
            WatchJobHandler.Invoke(null, new EventArgs() as ElapsedEventArgs);
            
            Assert.Equal(string.Empty, CaptureDeviceFilter);
        }

        [Fact]
        public void GivenUpdateProcessWatchToNoCapture_WhenStartCapture_ThenItRemovesProcessPortsFromWatchList()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, true);
            watchdog.UpdateProcessCaptureInWatchList(runningProcess.ProcessName, false);
            watchdog.StartCapture();
            
            WatchJobHandler.Invoke(null, new EventArgs() as ElapsedEventArgs);
            
            Assert.Equal(string.Empty, CaptureDeviceFilter);
        }

        [Fact]
        public void GivenProcessUnderWatch_WhenRemoveProcess_ThenItRemovesProperly()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            Assert.Equal(2, processWatches.Count());
            
            watchdog.RemoveProcessFromWatchList(runningProcess.ProcessName);
            processWatches = watchdog.GetProcessUnderWatch();
            
            Assert.Single(processWatches);
        }

        protected abstract IProcessWatcher GivenProcessWatcher();

    }
}