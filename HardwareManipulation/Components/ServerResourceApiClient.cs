using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Components
{
    public class ServerResourceApiClient : HttpClient
    {
        public ServerResourceApiClient() : base(new HttpClientHandler() { 
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } }, true)
        {

        }
    }
}
