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

        private string _appId;
        public string AppId
        {
            get => _appId;
            set => SetProperty(ref _appId, value);
        }

        private string _tentantUri;
        public string TentantUri
        {
            get => _tentantUri;
            set => SetProperty(ref _tentantUri, value);
        }

        private string _audience;
        public string Audience
        {
            get => _audience;
            set => SetProperty(ref _audience, value);
        }

        private string _redirectUri;
        public string RedirectUri
        {
            get => _redirectUri;
            set => SetProperty(ref _redirectUri, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private bool _useBroker;
        public bool UseBroker
        {
            get => _useBroker;
            set => SetProperty(ref _useBroker, value);
        }

        private Endpoint _selectedEndpoint;
        public Endpoint SelectedEndpoint
        {
            get => _selectedEndpoint;
            set 
            { 
                SetProperty(ref _selectedEndpoint, value);

                RedirectUri = _selectedEndpoint.RedirectUri;
                Audience = _selectedEndpoint.ResourceId;
                AppId = _selectedEndpoint.ApplicationId;
                TentantUri = _selectedEndpoint.Authority;
                UseBroker = _selectedEndpoint.UseBroker;
            }
        }

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
            Status = "No Token";
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

        private void SetTokenStatusMessage(string firstName,
                                 string lastName,
                                 DateTimeOffset expires)
        {
            Status = $"Token Received for {firstName} {lastName} that expires on {expires}";
        }
    }
}
