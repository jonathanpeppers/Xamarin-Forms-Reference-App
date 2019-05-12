﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Mobile.RefApp.CoreUI.Base;
using Mobile.RefApp.Lib.ADAL;

namespace Mobile.RefApp.CoreUI.ViewModels
{
    public class AzureTokenRawViewerViewModel
        : ViewModelBase
    {
        private CacheToken _cacheToken;
        public CacheToken CacheToken
        {
            get => _cacheToken;
            set => SetProperty(ref _cacheToken, value);
        }

        public AzureTokenRawViewerViewModel()
        {
            Title = "Raw Token Viewer";
        }

        public override async Task Initialize(Dictionary<string, object> navigationsParams = null)
        {
            await Task.Run(() =>
            {
                if (navigationsParams != null 
                && navigationsParams.ContainsKey("token"))
                {
                    CacheToken = navigationsParams["token"] as CacheToken;
                }
            }).ConfigureAwait(false);
        }
    }
}
