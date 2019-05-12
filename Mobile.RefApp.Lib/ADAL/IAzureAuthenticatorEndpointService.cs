using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mobile.RefApp.Lib.ADAL
{
    public interface IAzureAuthenticatorEndpointService
        : IAzureAuthenticatorService
    {
        Task<CacheToken> AuthenticateEndpoint(Endpoint endpoint,
                                                        [CallerMemberName] string memberName = "",
                                                        [CallerLineNumber] int lineNumber = 0);

        Task<CacheToken> AcquireTokenSilentAsync(Endpoint endpoint,
                                                [CallerMemberName] string memberName = "",
                                                [CallerLineNumber] int lineNumber = 0);


    }
}
