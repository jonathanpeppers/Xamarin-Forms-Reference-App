﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Mobile.RefApp.Lib.Logging;
using Mobile.RefApp.Lib.Network;

namespace Mobile.RefApp.Lib.ADAL
{
    public class AzureAuthenticatorService
		: IAzureAuthenticatorEndpointService
    {
        private readonly ILoggingService _loggingService;
        private readonly IAzurePlatformParameters _azurePlatformParameters;
        private readonly HttpClientHandler _httpClient;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public AzureAuthenticatorService(IAzurePlatformParameters azurePlatformParameters,
                                  IPlatformHttpClientHandler httpClientHandler,
                                  ILoggingService loggingService)
        {
            _azurePlatformParameters = azurePlatformParameters;
            _httpClient = httpClientHandler.CreateHandler();
            _loggingService = loggingService;
        }

        public async Task<CacheToken> Authenticate(string applicationId,
															 string authority,
															 string returnUri,
															 string resourceId,
															 bool useBroker = true,
															 [CallerMemberName] string memberName = "",
															 [CallerLineNumber] int lineNumber = 0)
        { 
			return await AuthenticateEndpoint(new Endpoint()
			{
				Authority = authority,
				ApplicationId = applicationId,
				Environment = Network.Environment.Production,
				IsActive = true,
				Name = string.Empty,
				RedirectUri = returnUri,
				ResourceId = resourceId,
				UseBroker = useBroker
			},
			memberName,
			lineNumber);
        }

        public async Task<CacheToken> AuthenticateEndpoint(Endpoint endpoint,
		  [CallerMemberName] string memberName = "",
		  [CallerLineNumber] int lineNumber = 0)


        {
            if (endpoint.IsActive)
            { 
                AuthenticationResult results = null;
                IPlatformParameters platformParameters = _azurePlatformParameters.GetPlatformParameters(endpoint.UseBroker);
                try
                {
                    var authContext = new AuthenticationContext(endpoint.Authority);
                    //fixes for security groups in iOS per
                    //https://aka.ms/adal-net-ios-keychain-access

#if iOS
                    authContext.iOSKeychainSecurityGroup = endpoint.iOSKeychainSecurityGroup;
#endif

                    _logBuilder.Clear();
					    LoggerCallbackHandler.LogCallback = AdalLog;
					    LoggerCallbackHandler.PiiLoggingEnabled = true;

					    _loggingService.LogInformation(typeof(AzureAuthenticatorService),
												   $"Before Acquiring Token values",
												   memberName,
												   lineNumber,
												   ComposePropertyValues(endpoint));
                    if (string.IsNullOrEmpty(endpoint.ExtraParameters))
                    {
                        results = await authContext.AcquireTokenAsync(endpoint.ResourceId,
                            endpoint.ApplicationId,
                            new Uri(endpoint.RedirectUri),
                            platformParameters).WithTimeout(5000);
                    }
                    else
                    {
                        results = await authContext.AcquireTokenAsync(endpoint.ResourceId,
                            endpoint.ApplicationId,
                            new Uri(endpoint.RedirectUri),
                            platformParameters, 
                            UserIdentifier.AnyUser,
                            endpoint.ExtraParameters).WithTimeout(5000);
                    }

                    _loggingService.LogInformation(typeof(AzureAuthenticatorService),
												   $"{Constants.AzureAuthenticator.AZURELOGTAG} Logs: {_logBuilder?.ToString()}");
                    _loggingService.LogInformation(typeof(AzureAuthenticatorService),
												  $"{Constants.AzureAuthenticator.AZURELOGTAG} Log: Access Token: {results?.AccessToken}\n\nAccess Token Type: {results?.AccessTokenType}\n\nExpires On: {results?.ExpiresOn.ToString()}");
                }
                catch (AdalUserMismatchException aume)
                { 
					    _loggingService.LogError(typeof(AzureAuthenticatorService),
											 (System.Exception)aume,
											 $"{Constants.AzureAuthenticator.AZURELOGERRORTAG} :AdalUserMismatchException",
											 memberName,
											 lineNumber,
											 $"Returned User: {aume?.ReturnedUser} Requested User: {aume?.RequestedUser}",
											 $"Error Code:: {aume?.ErrorCode}");
                }
                catch (AdalSilentTokenAcquisitionException astae)
                {
					    _loggingService.LogError(typeof(AzureAuthenticatorService),
											 (System.Exception)astae,
											 $"AdalSilientTokenAquisitionException {astae?.ErrorCode}",
											 memberName,
											 lineNumber,
											 astae.Data);
                }
                catch (AdalClaimChallengeException acce)
                {
					    _loggingService.LogError(typeof(AzureAuthenticatorService),
											(System.Exception)acce,
											$"AdalClaimsChallengeException:: Claims: {acce.Claims}",
											 memberName,
											 lineNumber,
											 acce.Data,
											 acce.Headers,
											 acce.ServiceErrorCodes,
											 acce.StatusCode);
                    throw;
                }
                catch (AdalServiceException ase)
                {
                    _loggingService.LogError(typeof(AzureAuthenticatorService),
											(System.Exception)ase,
											$"{Constants.AzureAuthenticator.AZURELOGERRORTAG}: AdalServiceException::",
											memberName,
											lineNumber,
											ase.Data,
											ase.Headers,
											ase.ServiceErrorCodes,
											ase.StatusCode);
                }
                catch (Exception e)
                { 
					    _loggingService.LogError(typeof(AzureAuthenticatorService),
											e,
											e.Message,
											memberName,
											lineNumber,
											null);
                }

                //
                //add result to token cache
                var cacheToken = AzureTokenCacheService.CreateCacheToken(results, endpoint);
                AzureTokenCacheService.AddToken(cacheToken);
                return cacheToken;
            }
            else
			  {
                throw new Exception("ERROR:  Endpoint not Active, please make Endpoint Active and try again.");
			  }
        }


        public async Task<CacheToken> AcquireTokenSilentAsync(Endpoint endpoint, 
                                                        [CallerMemberName] string memberName = "",
                                                        [CallerLineNumber] int lineNumber = 0)

        {
            AuthenticationResult results = null;
            try
            {
                var authContext = new AuthenticationContext(endpoint.Authority);
                //fixes for security groups in iOS per
                //https://aka.ms/adal-net-ios-keychain-access

#if iOS
                authContext.KeychainSecurityGroup = endpoint.iOSKeychainSecurityGroup;
#endif

                _logBuilder.Clear();
                LoggerCallbackHandler.LogCallback = AdalLog;
                LoggerCallbackHandler.PiiLoggingEnabled = true;

                _loggingService.LogInformation(typeof(AzureAuthenticatorService),
                                               $"Before Acquiring Silent Token values",
                                               memberName,
                                               lineNumber,
                                               ComposePropertyValues(endpoint));

                results = await authContext.AcquireTokenSilentAsync(endpoint.ResourceId, endpoint.ApplicationId);
            }
            catch (AdalUserMismatchException aume)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService),
                                         (System.Exception)aume,
                                         $"{Constants.AzureAuthenticator.AZURELOGERRORTAG} :AdalUserMismatchException",
                                         memberName,
                                         lineNumber,
                                         $"Returned User: {aume?.ReturnedUser} Requested User: {aume?.RequestedUser}",
                                         $"Error Code:: {aume?.ErrorCode}");
            }
            catch (AdalSilentTokenAcquisitionException astae)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService),
                                         (System.Exception)astae,
                                         $"AdalSilientTokenAquisitionException {astae?.ErrorCode}",
                                         memberName,
                                         lineNumber,
                                         astae.Data);
            }
            catch (AdalClaimChallengeException acce)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService),
                                        (System.Exception)acce,
                                        $"AdalClaimsChallengeException:: Claims: {acce.Claims}",
                                         memberName,
                                         lineNumber,
                                         acce.Data,
                                         acce.Headers,
                                         acce.ServiceErrorCodes,
                                         acce.StatusCode);

            }
            catch (AdalServiceException ase)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService),
                                        (System.Exception)ase,
                                        $"{Constants.AzureAuthenticator.AZURELOGERRORTAG}: AdalServiceException::",
                                        memberName,
                                        lineNumber,
                                        ase.Data,
                                        ase.Headers,
                                        ase.ServiceErrorCodes,
                                        ase.StatusCode);

            }
            catch (Exception e)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService),
                                        e,
                                        e.Message,
                                        memberName,
                                        lineNumber,
                                        null);
            }

            //add result to cache
            var cacheToken = AzureTokenCacheService.CreateCacheToken(results, endpoint);
            AzureTokenCacheService.AddToken(cacheToken);
            return cacheToken;
        }

        public IEnumerable<TokenCacheItem> GetCachedTokens(
            Endpoint endpoint,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            IEnumerable<TokenCacheItem> tokenCache = null;
            try
            {
                var authContext = new AuthenticationContext(endpoint.Authority);
                tokenCache = authContext.TokenCache.ReadItems();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService), ex, ex.Message);
            }

            return tokenCache;
        }

        public bool ClearCachedTokens(           
            Endpoint endpoint,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            bool results = false;
            try
            {
                var authContext = new AuthenticationContext(endpoint.Authority);
                authContext.TokenCache.Clear();
                results |= authContext.TokenCache.Count == 0;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(typeof(AzureAuthenticatorService), ex, ex.Message);
            }
            return results;
        }

        private void AdalLog(LogLevel level,
                             string message,
                             bool containsPii)
        {
            string pii = containsPii ? ":pii" : "";
            _logBuilder.AppendLine($"[{level}{pii}] {message}");
        }

        private object[] ComposePropertyValues(Endpoint endpoint)
        {
            return new List<object>
            {
                endpoint.ApplicationId,
                endpoint.Authority,
                endpoint.Environment,
                endpoint.IsActive,
                endpoint.Name,
                endpoint.ResourceId,
                endpoint.RedirectUri,
                endpoint.UseBroker,
            }.ToArray();
        }
    }
}
