using System;
using System.Collections.Generic;
using DesktopAssistant.BL.Hardware;
using Hardware.Connectors;
using Hardware.Models;
using Xunit;

namespace DesktopAssistant.Tests.Hardware.Tests.Connectors
{
    public abstract class ConnectorBaseTest
    {
        protected abstract KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected();

        protected abstract KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows();

        [Fact]
        public void GivenAcceptedResource_WhenGettingValue_ThenItReturnsExpectedValue()
        {
            var parameters = ProvideConnectorTargetsAndExpected();
            ConnectorBase connectorSubject = parameters.Key;
            IDictionary<MonitoringTarget, object> targetsAndExpected = parameters.Value;
            Dictionary<HardwareInformation, object> results = new Dictionary<HardwareInformation, object>();

            foreach (var target in targetsAndExpected)
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
    }
}