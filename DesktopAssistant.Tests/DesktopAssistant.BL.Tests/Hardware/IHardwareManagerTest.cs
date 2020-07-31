﻿using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.BL.Hardware;
using Hardware;
using Hardware.Connectors;
using Hardware.Models;
using Moq;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.Hardware
{
    public abstract class IHardwareManagerTest
    {
        [Fact]
        public void GivenInitialTargets_WhenInstantiateManager_ThenItSetsInitialTargets()
        {
            IHardwareManager manager = GivenHardwareManager();
            IEnumerable<MonitoringTarget> expected = GetConfigurationInitialTargets();

            IEnumerable<MonitoringTarget> actual = manager.GetInitialTargets();

            Assert.Empty(expected.Except(actual));
        }

        protected abstract IEnumerable<MonitoringTarget> GetConfigurationInitialTargets();
        protected abstract IEnumerable<MonitoringTarget> GetConfigurationAllTargets();
        protected abstract IHardwareManager GivenHardwareManager();
    }
}