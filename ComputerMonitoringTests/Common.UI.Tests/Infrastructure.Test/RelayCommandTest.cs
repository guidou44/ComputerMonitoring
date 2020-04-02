using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace ComputerMonitoringTests.Common.UI.Tests.Infrastructure.Test
{   
    public class RelayCommandTest
    {
        private string TEST_OUTPUT;
        private RelayCommand simpleCommandRelayerSubject;
        private RelayCommand<object> paramsCommandRelayerSubject;

        ICommand SimpleCommand
        {
            get { return simpleCommandRelayerSubject; }
        }

        ICommand ParamCommand
        {
            get { return paramsCommandRelayerSubject; }
        }

        public RelayCommandTest() { }

        [Fact]
        public void GivenSimpleCommand_WhenInvokingCommand3Times_ThenItExecute3Action()
        {
            TEST_OUTPUT = String.Empty;
            simpleCommandRelayerSubject = new RelayCommand(() => TEST_OUTPUT += "1");

            for (int i = 0; i < 3; i++)
                SimpleCommand.Execute(null);

            Assert.Equal("111", TEST_OUTPUT);
        }

        [Fact]
        public void GivenSimpleCommandWithCanExec_WhenInvokingCommand_ThenItDoesExecuteBasedOnOutsideState()
        {
            TEST_OUTPUT = String.Empty;
            bool canExecute = false;
            simpleCommandRelayerSubject = new RelayCommand(() => TEST_OUTPUT += "1", () => { return canExecute; });

            if (SimpleCommand.CanExecute(null))
            {
                for (int i = 0; i < 3; i++)
                    SimpleCommand.Execute(null);
            }
            Assert.NotEqual("111", TEST_OUTPUT);

            canExecute = true;
            SimpleCommand.Execute(null);

            Assert.Equal("1", TEST_OUTPUT);
        }

        [Fact]
        public void GivenSimpleCommandParams_WhenInvokingCommand3Times_ThenItExecute3Action()
        {
            TEST_OUTPUT = String.Empty;
            paramsCommandRelayerSubject = new RelayCommand<object>(s => TEST_OUTPUT += s as string);
            ;
            for (int i = 0; i < 3; i++)
                ParamCommand.Execute("5");

            Assert.Equal("555", TEST_OUTPUT);
        }

        [Fact]
        public void GivenSimpleCommandParamsAndCanExec_WhenInvokingCommand_ThenItExecuteBasedOnOutsideState()
        {
            string expected = "MODIFIED";
            TEST_OUTPUT = "NOT_MODIFIED";
            bool canExecute = false;
            paramsCommandRelayerSubject = new RelayCommand<object>(s => TEST_OUTPUT = s as string, s => { return canExecute; });

            if (ParamCommand.CanExecute(null))
            {
                ParamCommand.Execute(expected);
            }

            Assert.NotEqual(expected, TEST_OUTPUT);
            canExecute = true;

            if (ParamCommand.CanExecute(null))
            {
                ParamCommand.Execute(expected);
            }

            Assert.Equal(expected, TEST_OUTPUT);
        }
    }
}
