
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Mobile.RefApp.CoreUI.Base;
using Mobile.RefApp.CoreUI.Interfaces;
using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Keychain;
using Mobile.RefApp.Lib.Logging;
using Xamarin.Forms;

namespace Mobile.RefApp.CoreUI.ViewModels
{
    public class KeychainGroupListViewModel
        : ViewModelBase
    {
        private readonly ILoggingService _loggingService;
        private readonly IEndpointService _endpointService;
        private readonly IKeychainService _keychainService;

        private Endpoint _endpoint;

        private string _queryText;
        public string QueryText
        {
            get => _queryText;
            set => SetProperty(ref _queryText, value);
        }

        public ObservableCollection<string> KeychainKeys; 

        public ICommand SearchCommand { get; private set; }

        public KeychainGroupListViewModel(
            ILoggingService loggingService,
            IKeychainService keychainService,
            IEndpointService endpointService)
        {
            _loggingService = loggingService;
            _keychainService = keychainService;
            _endpointService = endpointService;

            Title = "Keychain Group Cache";
            KeychainKeys = new ObservableCollection<string>();
            SearchCommand = new Command(DoSearch);
        }

        public override async Task Initialize(Dictionary<string, object> navigationsParams = null)
        {
            try
            {
                var endpoints = await _endpointService.GetEndpointsByPlatform(App.CurrentPlatform);
                if (endpoints.Any())
                {
                    IsBusy = true;
                    _endpoint = endpoints[0];
                    QueryText = $"{_endpoint.iOSTeamId}.{_endpoint.iOSKeychainSecurityGroup}";
                    DoSearch();
                    IsBusy = false;
                } 
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError(typeof(KeychainGroupListViewModel), ex, ex.Message);
            }
        }

        private void DoSearch()
        {
            try
            {
                if (!string.IsNullOrEmpty(QueryText))
                {
                    KeychainKeys.Clear();
                    var results = _keychainService.GetRecordsFromKeychain(QueryText);
                    if (results.Any())
                    {
                        foreach (var result in results)
                            KeychainKeys.Add(result);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError(typeof(KeychainGroupListViewModel), ex, ex.Message);
            }
        }
    }
}
