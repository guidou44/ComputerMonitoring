using Common.UI.DialogServices.Exceptions;
using Common.UI.WindowProperty;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Exceptions;
using DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Fixtures;
using Xunit;

namespace DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests
{
    public abstract class IDialogServiceTest
    {
        private IDialogService _dialogServiceSubject;

        [Fact]
        public void GivenNewViewAndViewModel_WhenRegistering_ThenProperlyRegistered()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();

            Assert.True(_dialogServiceSubject.Mappings[typeof(DialogViewModelFixture)] == typeof(DialogViewFixture));
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenRegisteringToNewViewModel_ThenItThrowsProper()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            
            Assert.Throws<ArgumentException>(() => _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewInvalidFixture>());
        }

        [Fact]
        public void GivenNotRegisteredViewModel_WhenInstantiate_ThenItThrowsProper()
        {
            _dialogServiceSubject = ProvideDialogService();

            Assert.Throws<NotRegisteredViewModelException>(() => _dialogServiceSubject.Instantiate(new DialogViewModelFixture()));
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithOk_ThenItReturnsProper()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            _dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = _dialogServiceSubject.ShowDialog(vmFixture));
            vmFixture.RequestCloseWithOk();
            showTask.Wait();

            Assert.True(dialogResult);
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithCancel_ThenItReturnsProper()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            _dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = _dialogServiceSubject.ShowDialog(vmFixture));
            vmFixture.RequestCloseWithCancel();
            showTask.Wait();

            Assert.False(dialogResult);
        }

        [Fact]
        public void GivenAlreadyRegisteredView_WhenClosingWithX_ThenItClosesWindow()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();

            bool? dialogResult = null;
            _dialogServiceSubject.Instantiate(vmFixture);
            Task showTask = Task.Run(() => dialogResult = _dialogServiceSubject.ShowDialog(vmFixture));


            Assert.Throws<DialogClosedException>(() =>
            {
                vmFixture.RequestCloseWithX();
                showTask.Wait();
            });
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowException_ThenItUsesProperView()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            _dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            _dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewFixture>();



            Assert.Throws<ErrorOrMessageDialogShownException>(() => _dialogServiceSubject.ShowException(new Exception("TEST")));
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowMessage_ThenItUsesProperView()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            _dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            _dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewWithDataContextFixture>();

            Assert.Throws<ErrorOrMessageDialogShownException>(() => _dialogServiceSubject.ShowMessageBox("MESSAGE"));
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowException_ThenItUsesProperViewModel()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            _dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            _dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewFixture>();

            try
            {
                _dialogServiceSubject.ShowException(new Exception("TEST"));
            }
            catch (ErrorOrMessageDialogShownException e)
            {
                Assert.Equal(typeof(ErrorDialogViewModelFixture).Name, e.Message);
            }
        }

        [Fact]
        public void GivenMultipleRegisteredDialog_WhenShowMessageBox_ThenItUsesProperViewModel()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewFixture>();
            _dialogServiceSubject.Register<ErrorDialogViewModelFixture, DialogViewWithDataContextFixture>();
            _dialogServiceSubject.Register<MessageDialogViewModelFixture, DialogViewWithDataContextFixture>();

            try
            {
                _dialogServiceSubject.ShowMessageBox("MESSAGE");
            }
            catch (ErrorOrMessageDialogShownException e)
            {
                Assert.Equal(typeof(MessageDialogViewModelFixture).Name, e.Message);
            }
        }

        [Fact]
        public void GivenAlreadyRegisteredViewModel_WhenInstantiateNewOne_ItRemovesOldOne()
        {
            _dialogServiceSubject = ProvideDialogService();
            _dialogServiceSubject.Register<DialogViewModelFixture, DialogViewWithDataContextFixture>();
            DialogViewModelFixture vmFixture = new DialogViewModelFixture();
            DialogViewModelFixture vmFixture2 = new DialogViewModelFixture();

            _dialogServiceSubject.Instantiate(vmFixture);
            _dialogServiceSubject.Instantiate(vmFixture2);

            try
            {
                _dialogServiceSubject.ShowDialog(vmFixture2);
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
