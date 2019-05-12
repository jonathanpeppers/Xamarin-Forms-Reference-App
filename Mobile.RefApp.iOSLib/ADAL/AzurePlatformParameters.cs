using Mobile.RefApp.Lib.ADAL;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using UIKit;

namespace Mobile.RefApp.iOSLib.ADAL
{
	public class AzurePlatformParameters
			: IAzurePlatformParameters
	{
		public IPlatformParameters GetPlatformParameters(bool useBroker)
		{
			var controller = UIApplication.SharedApplication.KeyWindow.RootViewController;
			return new PlatformParameters(controller, useBroker);
		}
	}
}