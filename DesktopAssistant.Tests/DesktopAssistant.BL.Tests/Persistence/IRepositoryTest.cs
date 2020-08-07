using System;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.Exceptions;
using Hardware.Models;
using ProcessMonitoring.Models;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.Persistence
{
    public abstract class IRepositoryTest
    {
        [Fact]
        public void GivenValidHardwareRelatedEntity_WhenReadAllEntity_ThenItReturnsProperEntity()
        {
            IRepository repository = GivenRepository();

            ResourceCollection result = repository.Read<ResourceCollection>();
            
            Assert.NotNull(result);
            Assert.IsType<ResourceCollection>(result);
        }

        [Fact]
        public void GivenValidProcessWatchRelatedEntity_WhenReadEntity_ThenItReturnsProperEntity()
        {
            IRepository repository = GivenRepository();

            WatchdogInitialization result = repository.Read<WatchdogInitialization>();
            
            Assert.NotNull(result);
            Assert.IsType<WatchdogInitialization>(result);
        }

        [Fact]
        public void GivenInvalidEntity_WhenReadEntity_ThenItThrowsProperException()
        {
            IRepository repository = GivenRepository();

            Assert.Throws<NoFlatFileRepositoryDelegateException>(() => repository.Read<string>());
        }

        [Fact]
        public void GivenValidEntity_WhenUpdateEntity_ThenItUpdatesEntityProper()
        {
            IRepository repository = GivenRepository();
            Exception exception = new Exception("TEST");

            repository.Update(exception);
            AssertEntityUpdated(exception);
        }
        
        
        protected abstract IRepository GivenRepository();
        protected abstract void AssertEntityUpdated<TObject>(TObject entity);
    }
}