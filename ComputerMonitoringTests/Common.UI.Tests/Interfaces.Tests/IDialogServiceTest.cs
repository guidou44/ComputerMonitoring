using Common.UI.DialogServices.Exceptions;
using Common.UI.WindowProperty;
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
        public void GivenNotRegisteredViewModel_WhenInstantiate_ThenItThrowsProper()
        {
            dialogServiceSubject = ProvideDialogService();

            Assert.Throws<NotRegisteredViewModelException>(() => dialogServiceSubject.Instantiate(new DialogViewModelFixture()));
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithOk_ThenItReturnsProper()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = dialogServiceSubject.ShowDialog(vmFixture));
            vmFixture.RequestCloseWithOk();
            showTask.Wait();

            Assert.True(dialogResult);
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithCancel_ThenItReturnsProper()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = dialogServiceSubject.ShowDialog(vmFixture));
            vmFixture.RequestCloseWithCancel();
            showTask.Wait();

            Assert.False(dialogResult);
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithX_ThenItClosesWindow()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = dialogServiceSubject.ShowDialog(vmFixture));


            Assert.Throws<DialogClosedException>(() =>
            {
                vmFixture.RequestCloseWithX();
                showTask.Wait();
            });
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowException_ThenItUsesProperView()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewFixture>();



            Assert.Throws<ErrorOrMessageDialogShownException>(() => dialogServiceSubject.ShowException(new Exception("TEST")));
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowMessage_ThenItUsesProperView()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewWithDataContextFixture>();

            Assert.Throws<ErrorOrMessageDialogShownException>(() => dialogServiceSubject.ShowMessageBox("MESSAGE"));
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowException_ThenItUsesProperViewModel()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewFixture>();

            try
            {
                dialogServiceSubject.ShowException(new Exception("TEST"));
            }
            catch (ErrorOrMessageDialogShownException e)
            {
                Assert.Equal(typeof(ErrorDialogViewModelFixture).Name, e.Message);
            }
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowMessageBox_ThenItUsesProperViewModel()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewWithDataContextFixture>();

            try
            {
                dialogServiceSubject.ShowMessageBox("MESSAGE");
            }
            catch (ErrorOrMessageDialogShownException e)
            {
                Assert.Equal(typeof(MessageDialogViewModelFixture).Name, e.Message);
            }
        }

        [Fact]
        public void GivenAlreadyRegisteredViewModel_WhenInstantiateNewOne_ItRemovesOldOne()
        {
            dialogServiceSubject = ProvideDialogService();
            dialogServiceSubject.Register<DialogViewModelFixture, DialogViewWithDataContextFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();
            DialogViewModelFixture vmFixture2 = new DialogViewModelFixture();

            dialogServiceSubject.Instantiate(vmFixture);
            dialogServiceSubject.Instantiate(vmFixture2);

            try
            {
                dialogServiceSubject.ShowDialog(vmFixture2);
            }
            catch (ErrorOrMessageDialogShownException e)
            {
                Assert.Equal(vmFixture2.GetHashCode(), e.DataContext.GetHashCode());
                Assert.NotEqual(vmFixture.GetHashCode(), e.DataContext.GetHashCode());
            }
        }

        protected abstract IDialogService ProvideDialogService();
    }
}
