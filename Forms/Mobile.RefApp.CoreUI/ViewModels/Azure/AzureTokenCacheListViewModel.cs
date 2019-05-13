using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Mobile.RefApp.CoreUI.Base;
using Mobile.RefApp.CoreUI.Interfaces;
using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Logging;
using Xamarin.Forms;

namespace Mobile.RefApp.CoreUI.ViewModels
{
    public class AzureTokenCacheListViewModel
        : ViewModelBase
    {
        private readonly ILoggingService _loggingService;
        private readonly IAzureAuthenticatorEndpointService _authenticationService;
        private readonly IEndpointService _endpointService;

        public ObservableCollection<TokenCacheItem> CachedTokens { get; private set; }

        public ICommand ClearCachedTokensCommand { get; private set; }

        public AzureTokenCacheListViewModel(
            IAzureAuthenticatorEndpointService authenticationService,
            IEndpointService endpointService,
            ILoggingService loggingService)
        {
            _authenticationService = authenticationService;
            _endpointService = endpointService;
            _loggingService = loggingService;
            Title = "Token Cache";

            CachedTokens = new ObservableCollection<TokenCacheItem>();
            ClearCachedTokensCommand = new Command(async () => await DoClearCachedTokens());
        }

        public override async Task Initialize(
            Dictionary<string, object> navigationsParams = null)
        {
            try
            {
                var endpoints = await _endpointService.GetEndpointsByPlatform(App.CurrentPlatform);
                if (endpoints.Any())
                {
                    var endpoint = endpoints[0];
                    if (endpoint != null)
                    {
                        var tokens = _authenticationService.GetCachedTokens(endpoint);
                        foreach (var token in tokens)
                            CachedTokens.Add(token);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError(typeof(AzureTokenCacheListViewModel), ex, ex.Message);
            }
        }
        private async Task DoClearCachedTokens()
        {
            try
            {
                var endpoints = await _endpointService.GetEndpointsByPlatform(App.CurrentPlatform);
                if (endpoints.Any())
                {
                    var endpoint = endpoints[0];
                    if (endpoint != null)
                    {
                        var results = _authenticationService.ClearCachedTokens(endpoint);
                        CachedTokens.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError(typeof(AzureTokenCacheListViewModel), ex, ex.Message);
            }
        }
    }
}
