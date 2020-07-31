using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DesktopAssistant.BL.Hardware;
using Hardware.Connectors;
using Hardware.ServerDTOs.Models;
using Hardware.Wrappers;
using Moq;
using Newtonsoft.Json;

namespace DesktopAssistant.Tests.Hardware.Tests.Connectors
{
    public class ASPNET_API_ConnectorTest : ConnectorBaseTest
    {
        private const double expectedCpuload = 69.0;
        private const double expectedRamUsage = 70.0;
        private const double expectedCpuTemperature = 71.0;

        protected override KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>> ProvideConnectorTargetsAndExpected()
        {
            ASPNET_API_Connector connector = GetConnector();
            IDictionary<MonitoringTarget, object> targetAndExpected = new Dictionary<MonitoringTarget, object>()
            {
                { MonitoringTarget.Server_CPU_Load,  expectedCpuload },
                { MonitoringTarget.Server_RAM_Usage,  expectedRamUsage },
                { MonitoringTarget.Server_CPU_Temp,  expectedCpuTemperature }
            };

            return new KeyValuePair<ConnectorBase, IDictionary<MonitoringTarget, object>>(connector, targetAndExpected);
        }

        protected override KeyValuePair<ConnectorBase, MonitoringTarget> ProvideConnectorWithTargetThatThrows()
        {
            ASPNET_API_Connector connector = GetConnector();
            return new KeyValuePair<ConnectorBase, MonitoringTarget>(connector, MonitoringTarget.Server_CPU_ProcessUsage);
        }

        private ASPNET_API_Connector GetConnector()
        {
            Uri baseAddressUri = new Uri("http://some.uri");
            Uri cpuUri = new Uri(baseAddressUri, "/api/GeneralUsage/Cpu");
            Uri ramUri = new Uri(baseAddressUri, "/api/GeneralUsage/Ram");
            Uri tempUri = new Uri(baseAddressUri, "/api/Temperature/Cpu");
            Uri processUri = new Uri(baseAddressUri, "/api/Process/Cpu");

            HttpContent cpuContent = new StreamContent(GetStringDtoWithValue(expectedCpuload, "CPU_TEST", "CPU_UNIT"));            
            HttpContent ramContent = new StreamContent(GetStringDtoWithValue(expectedRamUsage, "RAM_TEST", "RAM_UNIT"));
            HttpContent tempContent = new StreamContent(GetStringDtoWithValue(expectedCpuTemperature, "TEMP_TEST", "TEMP_UNIT"));

            cpuContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            ramContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            tempContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage cpuResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = cpuContent};
            HttpResponseMessage ramResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = ramContent };
            HttpResponseMessage tempResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = tempContent };



            Mock<ServerResourceApiClientWrapper> apiClient = new Mock<ServerResourceApiClientWrapper>();
            apiClient.SetupGet(a => a.BaseAddress).Returns(baseAddressUri);

            apiClient.Setup(a => a.GetAsync(cpuUri)).ReturnsAsync(cpuResponse);
            apiClient.Setup(a => a.GetAsync(ramUri)).ReturnsAsync(ramResponse);
            apiClient.Setup(a => a.GetAsync(tempUri)).ReturnsAsync(tempResponse);

            return new ASPNET_API_Connector(apiClient.Object);
        }

        private Stream GetStringDtoWithValue(double dtoValue, string shortName, string unit)
        {
            ServerResourceDTO dto = new ServerResourceDTO();
            dto.Value = dtoValue;
            dto.Resource_Type = new ResourceTypeDTO();
            dto.Resource_Type.Short_Name = shortName;
            dto.Server_Resource_Unit = new ServerResourceUnitDTO();
            dto.Server_Resource_Unit.Unit = unit;
            dto.Sample_Time = new SampleTimeDTO();
            dto.Process = new ProcessDTO();

            MemoryStream stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, new UTF8Encoding(true), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, dto);
                jtw.Flush();
            }
          
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
