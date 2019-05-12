using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mobile.RefApp.CoreUI.Base;
using Mobile.RefApp.CoreUI.Interfaces;
using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Logging;
using Xamarin.Forms.Internals;

namespace Mobile.RefApp.CoreUI.ViewModels
{
    public class AzureTokenGeneratorViewModel 
        : ViewModelBase
    {
        private readonly ILoggingService _loggingService;
        private readonly IAzureAuthenticatorEndpointService _authenticationService;
        private readonly IEndpointService _endpointService;

        public ObservableCollection<Endpoint> EndPoints { get; set; }

        public AzureTokenGeneratorViewModel(
            ILoggingService loggingService, 
            IAzureAuthenticatorEndpointService authenticationService,
            IEndpointService endpointService)
        {
            _loggingService = loggingService;
            _authenticationService = authenticationService;
            _endpointService = endpointService;
            EndPoints = new ObservableCollection<Endpoint>();

            Title = "Token Generator";
        }

        public async override Task Initialize(Dictionary<string, object> navigationsParams = null)
        {
            var endpoints = await _endpointService.GetEndpointsByPlatorm(App.CurrentPlatform);
            if(endpoints.Any())
            {
                endpoints.ForEach(x => EndPoints.Add(x));
            }
            await base.Initialize(navigationsParams);
        }
    }
}
