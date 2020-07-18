using Common.UI.Infrastructure;
using DesktopAssistantTests.Common.UI.Tests.Infrastructure.Test.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Xunit;

namespace DesktopAssistantTests.Common.UI.Tests.Infrastructure.Test
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

        private object _randomProperty;
        public object RandomProperty
        {
            get { return _randomProperty; }
            set
            {
                _randomProperty = value;
                if (simpleCommandRelayerSubject != null)
                    simpleCommandRelayerSubject.RaiseCanExecuteChanged();
                if (paramsCommandRelayerSubject != null)
                    paramsCommandRelayerSubject.RaiseCanExecuteChanged();
            }
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
        public void GivenSimpleCommand_WhenInvokingCommandWithNullAction_ThenItThrowsProper()
        {
            Assert.Throws<ArgumentNullException>(() => (simpleCommandRelayerSubject = new RelayCommand(null, () => { return true; })));
            Assert.Throws<ArgumentNullException>(() => (paramsCommandRelayerSubject = new RelayCommand<object>(null, o => { return true; })));
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

        [Fact]
        public void GivenSimpleCommandWithCanExec_WhenRegisteringEvent_ThenItReactWithProperHandler()
        {
            TEST_OUTPUT = String.Empty;
            simpleCommandRelayerSubject = new RelayCommand(() => TEST_OUTPUT += "1", () => false);

            simpleCommandRelayerSubject.CanExecuteChanged += (sender, args) => TEST_OUTPUT += "2";
            RandomProperty = new object();
            DispatcherTestHelper.ProcessWorkItems(DispatcherPriority.Background);

            Assert.Equal("2", TEST_OUTPUT);
        }

        [Fact]
        public void GivenParamCommandWithCanExec_WhenRegisteringEvent_ThenItReactWithProperHandler()
        {
            TEST_OUTPUT = String.Empty;
            paramsCommandRelayerSubject = new RelayCommand<object>(o => TEST_OUTPUT += "1", o => false);

            paramsCommandRelayerSubject.CanExecuteChanged += (sender, args) => TEST_OUTPUT += "5";
            RandomProperty = new object();
            DispatcherTestHelper.ProcessWorkItems(DispatcherPriority.Background);

            Assert.Equal("5", TEST_OUTPUT);
        }

        [Fact]
        public void GivenSimpleCommandWithCanExec_WhenUnRegisteringEvent_ThenItDoesNothing()
        {
            TEST_OUTPUT = String.Empty;
            simpleCommandRelayerSubject = new RelayCommand(() => TEST_OUTPUT += "1", () => false);
            EventHandler handler = (sender, args) => TEST_OUTPUT += "5";
            simpleCommandRelayerSubject.CanExecuteChanged += handler;
            simpleCommandRelayerSubject.CanExecuteChanged -= handler;

            RandomProperty = new object();
            DispatcherTestHelper.ProcessWorkItems(DispatcherPriority.Background);

            Assert.Equal(String.Empty, TEST_OUTPUT);
        }

        [Fact]
        public void GivenParamCommandWithCanExec_WhenUnRegisteringEvent_ThenItDoesNothing()
        {
            TEST_OUTPUT = String.Empty;
            paramsCommandRelayerSubject = new RelayCommand<object>(o => TEST_OUTPUT += "1", o => false);
            EventHandler handler = (sender, args) => TEST_OUTPUT += "5";
            paramsCommandRelayerSubject.CanExecuteChanged += handler;
            paramsCommandRelayerSubject.CanExecuteChanged -= handler;

            RandomProperty = new object();
            DispatcherTestHelper.ProcessWorkItems(DispatcherPriority.Background);

            Assert.Equal(String.Empty, TEST_OUTPUT);
        }

        [Fact]
        public void GivenSimpleCommand_WhenCheckingForCanExec_ThenItReturnsTrue()
        {
            simpleCommandRelayerSubject = new RelayCommand(() => TEST_OUTPUT += "1");

            Assert.True(SimpleCommand.CanExecute(null));
        }

        [Fact]
        public void GivenParamCommand_WhenCheckingForCanExec_ThenItReturnsTrue()
        {
            paramsCommandRelayerSubject = new RelayCommand<object>(o => TEST_OUTPUT += "1");

            Assert.True(ParamCommand.CanExecute(null));
        }
    }
}
