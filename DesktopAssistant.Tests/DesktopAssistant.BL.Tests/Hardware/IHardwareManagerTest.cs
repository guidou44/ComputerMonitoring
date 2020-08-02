using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.BL.Hardware;
using Hardware;
using Hardware.Connectors;
using Hardware.Exceptions;
using Hardware.Models;
using Moq;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.Hardware
{
    public abstract class IHardwareManagerTest
    {
        [Fact]
        public void GivenInitialTargetsConfig_WhenInstantiateManager_ThenItSetsInitialTargets()
        {
            IHardwareManager manager = GivenHardwareManager();
            IEnumerable<MonitoringTarget> expected = GetConfigurationInitialTargets();

            IEnumerable<MonitoringTarget> actual = manager.GetInitialTargets();

            Assert.Empty(expected.Except(actual));
        }

        [Fact]
        public void GivenTargetConfig_WhenGetAllTargets_ThenItOmitsNotReachableTargets()
        {
            IHardwareManager manager = GivenHardwareManager();
            IEnumerable<MonitoringTarget> allTargets = GetConfigurationAllTargets();
            MonitoringTarget notSupported = GetNotSupportedTarget();

            IEnumerable<MonitoringTarget> actual = manager.GetAllTargets();

            Assert.NotEmpty(allTargets.Except(actual));
            Assert.Contains(notSupported, allTargets.Except(actual));
        }

        [Fact]
        public void GivenSupportedTarget_WhenGetCalculatedValue_ThenItReturnsProperValue()
        {
            IHardwareManager manager = GivenHardwareManager();
            IEnumerable<MonitoringTarget> initial = manager.GetInitialTargets();

            IHardwareInfo hardwareInfo = manager.GetCalculatedValue(initial.First());
            
            Assert.NotNull(hardwareInfo);
            Assert.NotNull(hardwareInfo.MainValue);
            Assert.NotEmpty(hardwareInfo.ShortName);
            Assert.NotEmpty(hardwareInfo.UnitSymbol);
        }

        [Fact]
        public void GivenNotSupportedTarget_WhenGetCalculatedValue_ThenItThrowsProper()
        {
            IHardwareManager manager = GivenHardwareManager();
            MonitoringTarget notSupported = GetNotSupportedTarget();

            Assert.Throws<HardwareCommunicationException>(() => manager.GetCalculatedValue(notSupported));
        }

        [Fact]
        public void GivenMultipleSupportedTargets_WhenGetCalculatedValues_ThenItReturnsProperValue()
        {
            IHardwareManager manager = GivenHardwareManager();
            IEnumerable<MonitoringTarget> expected = GetConfigurationInitialTargets();

            IEnumerable<IHardwareInfo> values = manager.GetCalculatedValues(expected.ToList());

            Assert.All(values, v =>
            {
                Assert.NotNull(v);
                Assert.NotNull(v.MainValue);
                Assert.NotEmpty(v.ShortName);
                Assert.NotEmpty(v.UnitSymbol);
            });
        }

        [Fact]
        public void GivenMultipleSupportedTargetsWithOneNotSupported_WhenGetCalculatedValues_ThenItReturnsProperValue()
        {
            IHardwareManager manager = GivenHardwareManager();
            ICollection<MonitoringTarget> expected = GetConfigurationInitialTargets().ToList();
            MonitoringTarget notSupported = GetNotSupportedTarget();
            expected.Add(notSupported);
            int initialCount = expected.Count();

            IEnumerable<IHardwareInfo> values = manager.GetCalculatedValues(expected);

            Assert.All(values, v =>
            {
                Assert.NotNull(v);
                Assert.NotNull(v.MainValue);
                Assert.NotEmpty(v.ShortName);
                Assert.NotEmpty(v.UnitSymbol);
            });
            Assert.Equal(initialCount - 1, expected.Count());
            Assert.DoesNotContain(notSupported, expected);
        }

        protected abstract IEnumerable<MonitoringTarget> GetConfigurationInitialTargets();
        protected abstract IEnumerable<MonitoringTarget> GetConfigurationAllTargets();
        protected abstract IHardwareManager GivenHardwareManager();
        protected abstract MonitoringTarget GetNotSupportedTarget();
    }
}