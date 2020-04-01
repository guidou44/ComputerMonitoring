using Common.UI.Interfaces;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces.Exceptions;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces.Fixtures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces
{
    public abstract class IDialogServiceTest
    {
        private IDialogService dialogServiceSubject;

        [Fact]
        public void GivenNewViewAndViewModel_WhenRegistering_ThenProperlyRegistered()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();

            Assert.True(dialogServiceSubject.Mappings[typeof(DialogViewModelFixture)] == typeof(DialogViewFixture));
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenRegisteringToNewViewModel_ThenItThrowsProper()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            
            Assert.Throws<ArgumentException>(() => dialogServiceSubject.Register<DialogViewModelFixture, DialogViewInvalidFixture>());
        }

        [Fact]
        public async void GivenAlreadyRegisteredView_WhenClosingWithOk_ThenItReturnsProper()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture fixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            Task showTask = Task.Run(() => dialogResult = dialogServiceSubject.ShowDialog(fixture));
            await Task.Delay(1);
            fixture.RequestCloseWithOk();
            showTask.Wait();

            Assert.True(dialogResult);
        }

        [Fact]
        public async void GivenAlreadyRegisteredView_WhenClosingWithCancel_ThenItReturnsProper()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture fixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            Task showTask = Task.Run(() => dialogResult = dialogServiceSubject.ShowDialog(fixture));
            await Task.Delay(1);
            fixture.RequestCloseWithCancel();
            showTask.Wait();

            Assert.False(dialogResult);
        }

        protected abstract IDialogService ProvideDialogService();

    }
}
