using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Mobile.RefApp.Lib.ADAL
{
    public static class AzureTokenCacheService
    {
        private static readonly IList<CacheToken> _tokens = _tokens ?? new List<CacheToken>();

        public static IList<CacheToken> GetCacheTokens()
        {
            return _tokens;
        }

        public static void AddToken(CacheToken token)
        {
            CacheToken _removeToken = null;

            foreach (var t in _tokens)
            {
                if (t.Name == token.Name
                    && t.ResourceId == token.ResourceId
                    && t.TenantId == token.TenantId)
                {
                    _removeToken = t;
                }
            }

            if (_removeToken != null)
                _tokens.Remove(_removeToken);

            _tokens.Add(token);
        }

        public static void RemoveToken(CacheToken token)
        {
            if (_tokens.Contains(token))
            {
                _tokens.Remove(token);
            }
        }

        public static bool Clear()
        {
            var result = false;
            try
            {
                _tokens.Clear();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public static CacheToken CreateCacheToken(AuthenticationResult result, Endpoint endpoint)
        {
            return new CacheToken
            {
                 Endpoint = endpoint,
                 ExpiresOn = result.ExpiresOn,
                 UserInfo = result.UserInfo,
                 Token = result.AccessToken,
                 TenantId = result.TenantId
            };
        }

        public static CacheToken GetTokenByEndpoint(Endpoint endpoint)
        {
            return _tokens.SingleOrDefault(x => x.Endpoint.Name == endpoint.Name);
        }

        public static CacheToken GetTokenByName(string name)
        {
            return _tokens.SingleOrDefault(x => x.Name == name);
        } 

        public static IList<CacheToken> GetTokenByUpn(string upn)
        {
            return _tokens.Where(x => x.UserInfo.DisplayableId == upn).ToList();
        }
    }
}
