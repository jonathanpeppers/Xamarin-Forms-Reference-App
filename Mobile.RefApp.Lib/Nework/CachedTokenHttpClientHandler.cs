using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Logging;

namespace Mobile.RefApp.Lib.Network
{
    public class CachedTokenHttpClientHandler 
        : HttpClientHandler
    {
        private readonly IAzureAuthenticatorEndpointService _endpointService;
        private readonly ILoggingService _loggingService;
        private readonly Lib.ADAL.Endpoint _endpoint;

        public CachedTokenHttpClientHandler(ILoggingService loggingService,
                                            IAzureAuthenticatorEndpointService endpointService,
                                                 Lib.ADAL.Endpoint endpoint)
                                                
        {
            _endpointService = endpointService;
            _loggingService = loggingService;
            _endpoint = endpoint;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
                                                                     CancellationToken cancellationToken)
        {
            try
            {
                var token = await _endpointService.AcquireTokenSilentAsync(_endpoint); 

                //if old token exist - get rid of it
                if (request.Headers.Contains("Authorization"))
                    request.Headers.Remove("Authorization");

                request.Headers.Add("Authorization", $"Bearer {token.Token}");
            }
            catch(Exception ex)
            {
                _loggingService.LogError(typeof(CachedTokenHttpClientHandler), ex, ex.Message);
            }
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
