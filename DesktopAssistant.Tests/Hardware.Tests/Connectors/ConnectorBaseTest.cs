﻿using Hardware.Connectors;
using Hardware.Enums;
using Hardware.Helpers;
using Hardware.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.Hardware.Connectors
{
    public abstract class ConnectorBaseTest
    {
        [Fact]
        public void GivenAcceptedResource_WhenGettingValue_ThenItReturnsExpectedValue()
        {
            var parameters = ProvideConnectorTargetsAndExpected();
            ConnectorBase connectorSubject = parameters.Key;
            IDictionary<MonitoringTarget, object> targetsAndexpected = parameters.Value;
            Dictionary<HardwareInformation, object> results = new Dictionary<HardwareInformation, object>();

            foreach (var target in targetsAndexpected)
                results.Add(connectorSubject.GetValue(target.Key), target.Value);

            Assert.All(results, r =>
            {
                Assert.NotNull(r.Key.MainValue);
                Assert.Equal(r.Key.MainValue, r.Value);
            });
        }

        [Fact]
        public void GivenNotAcceptedResource_WhenGettingValue_ThenItThrowsProper()
        {
            KeyValuePair<ConnectorBase, MonitoringTarget> given = ProvideConnectorWithTargetThatThrows();

            Assert.Throws<NotImplementedException>(() => given.Key.GetValue(given.Value));
        }

        protected abstract KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected();

        protected abstract KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows();


    }
}
