using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class ServerResourceApiClientWrapper : HttpClient
    {
        public ServerResourceApiClientWrapper() : base(new HttpClientHandler() { 
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } }, true)
        {

        }

        public new virtual Uri BaseAddress 
        {
            get { return base.BaseAddress; }
            set { base.BaseAddress = value; }
        }

        public new virtual Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return base.GetAsync(requestUri);
        }
    }
}
