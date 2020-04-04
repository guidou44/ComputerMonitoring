using HardwareAccess.Enums;
using HardwareAccess.Models;
using HardwareAccess.ServerDTOs.Models;
using HardwareManipulation.Components;
using HardwareManipulation.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Connectors
{
    public class ASPNET_API_Connector : ConnectorBase
    {
        private const string SERVER_ADDRESS = "https://192.168.50.107";
        private static ServerResourceApiClientWrapper _client;

        public ASPNET_API_Connector(ServerResourceApiClientWrapper client)
        {
            InitializeClient(client);
        }

        #region private Methods

        

        private Uri GetUri(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.Server_CPU_Load:
                    return new Uri(_client.BaseAddress, "/api/GeneralUsage/Cpu");
                case MonitoringTarget.Server_RAM_Usage:
                    return new Uri(_client.BaseAddress, "/api/GeneralUsage/Ram");
                case MonitoringTarget.Server_CPU_Temp:
                    return new Uri(_client.BaseAddress, "/api/Temperature/Cpu");
                case MonitoringTarget.Server_CPU_ProcessUsage:
                    return new Uri(_client.BaseAddress, "/api/Process/Cpu");
                default:
                    throw new NotImplementedException($"No Specific URI for resource'{ressource.ToString()}'");
            }
        }

        private void InitializeClient(ServerResourceApiClientWrapper client)
        {
            _client = client;
            _client.BaseAddress = new Uri(SERVER_ADDRESS);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        private async Task<ServerResourceDTO> GetSingleResourceInfo(MonitoringTarget target)
        {
            var uri = GetUri(target);

            using (var response = await _client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var output = await response.Content.ReadAsAsync<ServerResourceDTO>();
                    return output;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private HardwareInformation MapDTO2Model(ServerResourceDTO dto)
        {
            return new HardwareInformation()
            {
                MainValue = Math.Round(dto.Value, 2),
                ShortName = "S." + dto.Resource_Type.Short_Name.Split('_')[0],
                UnitSymbol = dto.Server_Resource_Unit.Unit
            };

        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget resource)
        {
            switch (resource)
            {
                case MonitoringTarget.Server_CPU_Load:
                case MonitoringTarget.Server_CPU_Temp:
                case MonitoringTarget.Server_RAM_Usage:
                    var resultDTO = Task.Run(() => GetSingleResourceInfo(resource)).Result;
                    return MapDTO2Model(resultDTO);

                default:
                    throw new NotImplementedException($"Monitoring target '{resource}' not implemented for connector {nameof(ASPNET_API_Connector)}");
            }
        }
    }
}
