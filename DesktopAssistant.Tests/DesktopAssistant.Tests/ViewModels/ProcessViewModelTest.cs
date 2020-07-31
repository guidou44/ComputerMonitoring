using System;
using Common.Reports;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using DesktopAssistant.ViewModels;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.ViewModels
{
    public class ProcessViewModelTest
    {
        [Fact]
        public void GivenProcessName_WhenFormatToString_ThenItFormatsProper()
        {
            ProcessViewModel processVmSubject2Upper = new ProcessViewModel(true, "Test_ProcesS");
            Assert.Equal("TS", processVmSubject2Upper.ToString());

            ProcessViewModel processVmSubject1Upper = new ProcessViewModel(true, "test_procesS");
            Assert.Equal("S", processVmSubject1Upper.ToString());

            ProcessViewModel processVmSubjectNoUpper = new ProcessViewModel(true, "test_process");
            Assert.Equal("t", processVmSubjectNoUpper.ToString());
        }

        [Fact]
        public void GivenProcessVm_WhenInvokeChangeCommandWithNullProcess_ThenProperEventIsRaised()
        {
            const string expectedNewProcessName = "NEW PROCESS";
            string receivedNewProcessName = String.Empty;
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process");
            EventHandler processNameChangedTestHandler = new EventHandler((s, a) => receivedNewProcessName = (s as ProcessViewModel).ProcessName);
            processVmSubject.OnProcessNameChangedEvent += processNameChangedTestHandler;
            processVmSubject.ProcessName = expectedNewProcessName;

            Assert.True(processVmSubject.ChangeWatchdogTargetCommand.CanExecute(null));
            processVmSubject.ChangeWatchdogTargetCommand.Execute(null);

            Assert.Equal(expectedNewProcessName, receivedNewProcessName);
            processVmSubject.OnProcessNameChangedEvent -= processNameChangedTestHandler;
        }

        [Fact]
        public void GivenProcessVm_WhenInvokeRemoveCommand_ThenProperEventIsRaised()
        {
            ProcessViewModel receivedProcessToRemove = null;
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process");
            EventHandler processNameChangedTestHandler = new EventHandler((s, a) => receivedProcessToRemove = (s as ProcessViewModel));
            processVmSubject.OnProcessWatchRemoveEvent += processNameChangedTestHandler;

            processVmSubject.RemoveProcessWatchCommand.Execute(null);

            Assert.Equal(processVmSubject, receivedProcessToRemove);
            processVmSubject.OnProcessNameChangedEvent -= processNameChangedTestHandler;
        }

        [Fact]
        public void GivenProcessVm_WhenToggleWatchState_ThenWatchStateChanges()
        {
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process");
            bool initialState = processVmSubject.DoCapture;

            processVmSubject.ToggleWatchdogRunStateCommand.Execute(null);

            Assert.NotEqual(initialState, processVmSubject.DoCapture);
        }
    }
}