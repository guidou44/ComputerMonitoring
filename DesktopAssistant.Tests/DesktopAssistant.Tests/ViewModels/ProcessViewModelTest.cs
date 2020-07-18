using Common.MailClient;
using Common.Reports;
using DesktopAssistantTests.DesktopAssistantTests.Helper;
using DesktopAssistantTests.ProcessMonitoringTests.Helpers;
using DesktopAssistant.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.DesktopAssistantTests.ViewModels
{
    public class ProcessViewModelTest
    {
        [Fact]
        public void GivenProcessName_WhenFormatToString_ThenItFormatsProper()
        {
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject2Upper = new ProcessViewModel(true, "Test_ProcesS", reporter) { Process = null };
            Assert.Equal("TS", processVmSubject2Upper.ToString());

            ProcessViewModel processVmSubject1Upper = new ProcessViewModel(true, "test_procesS", reporter) { Process = null };
            Assert.Equal("S", processVmSubject1Upper.ToString());

            ProcessViewModel processVmSubjectNoUpper = new ProcessViewModel(true, "test_process", reporter) { Process = null };
            Assert.Equal("t", processVmSubjectNoUpper.ToString());
        }

        [Fact]
        public void GivenProcessVm_WhenToggleWatchState_ThenWatchStateChanges()
        {
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process", reporter) { Process = null };
            bool initialState = processVmSubject.Check4PacketExchange;

            processVmSubject.ToggleWatchdogRunStateCommand.Execute(null);

            Assert.NotEqual(initialState, processVmSubject.Check4PacketExchange);
        }

        [Fact]
        public void GivenProcessVm_WhenInvokeRemoveCommand_ThenProperEventIsRaised()
        {
            ProcessViewModel receivedProcessToRemove = null;
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process", reporter) { Process = null };
            processVmSubject.OnProcessWatchRemoveEvent += new EventHandler((s, a) => receivedProcessToRemove = (s as ProcessViewModel));

            processVmSubject.RemoveProcessWatchCommand.Execute(null);

            Assert.Equal(processVmSubject, receivedProcessToRemove);
            processVmSubject.OnProcessNameChangedEvent -= (s, a) => receivedProcessToRemove = (s as ProcessViewModel);
        }

        [Fact]
        public void GivenProcessVm_WhenInvokeChangeCommandWithNullProcess_ThenProperEventIsRaised()
        {
            const string expectedNewProcessName = "NEW PROCESS";
            string receivedNewProcessName = String.Empty;
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process", reporter) { Process = null };
            processVmSubject.OnProcessNameChangedEvent += new EventHandler((s, a) => receivedNewProcessName = (s as ProcessViewModel).ProcessName);
            processVmSubject.ProcessName = expectedNewProcessName;

            Assert.True(processVmSubject.ChangeWatchdogTargetCommand.CanExecute(null));
            processVmSubject.ChangeWatchdogTargetCommand.Execute(null);

            Assert.Equal(expectedNewProcessName, receivedNewProcessName);
            processVmSubject.OnProcessNameChangedEvent -= (s, a) => receivedNewProcessName = (s as ProcessViewModel).ProcessName;
        }

        [Fact]
        public void GivenProcessVm_WhenInvokeChangeCommandWithNonNullProcess_ThenProperEventIsRaised()
        {
            const string expectedNewProcessName = "NEW PROCESS";
            string receivedNewProcessName = String.Empty;
            Reporter reporter = ComputerMonitoringTestHelper.GivenReporter();
            ProcessViewModel processVmSubject = new ProcessViewModel(true, "Test_Process", reporter)
            { Process = WatchDogTestHelper.GivenFirstRunningProcess() };
            processVmSubject.OnProcessNameChangedEvent += new EventHandler((s, a) => receivedNewProcessName = (s as ProcessViewModel).ProcessName);
            processVmSubject.ProcessName = expectedNewProcessName;

            Assert.True(processVmSubject.ChangeWatchdogTargetCommand.CanExecute(null));
            processVmSubject.ChangeWatchdogTargetCommand.Execute(null);

            Assert.Equal(expectedNewProcessName, receivedNewProcessName);
            processVmSubject.OnProcessNameChangedEvent -= (s, a) => receivedNewProcessName = (s as ProcessViewModel).ProcessName;
        }

        [Fact]
        public void GivenNotRunningProcess_WhenChangeRunState_ThenItReportsProcessStart()
        {
            const string processName = "TEST_PROCESS";
            string messageSubject = String.Empty;            
            string expectedMessageSubject = $"ALARM: Detected process start for {processName}";
            Mock<IMailClient> emailClient = ComputerMonitoringTestHelper.ProvideSmtpClient();
            emailClient.Setup(s => s.Send(It.IsAny<MailMessage>())).Callback<MailMessage>(m => messageSubject = m.Subject);
            Reporter reporter = new Reporter(emailClient.Object);
            ProcessViewModel processVmSubject = new ProcessViewModel(true, processName, reporter) { IsRunning = false };

            processVmSubject.IsRunning = true;

            Assert.Equal(expectedMessageSubject, messageSubject);
        }
    }
}
