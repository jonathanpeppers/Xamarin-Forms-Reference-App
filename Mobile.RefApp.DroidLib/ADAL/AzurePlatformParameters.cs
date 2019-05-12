using System;

using Android.App;

using Mobile.RefApp.Lib.ADAL;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Mobile.RefApp.DroidLib.ADAL
{
	public class AzurePlatformParameters
		   : IAzurePlatformParameters
	{
		public Activity CurrentContext { get; set; }

		public AzurePlatformParameters()
		{
            CurrentContext = null; 
		}

		public AzurePlatformParameters(Activity context)
		{
			CurrentContext = context;
		}

		public IPlatformParameters GetPlatformParameters(bool useBroker)
		{
			if (CurrentContext != null)
				return new PlatformParameters(CurrentContext, useBroker);
			else
				throw new Exception(Lib.ADAL.Constants.AzurePlatformParameters.ANDROIDCURRENTCONTEXTNULL);
		}
	}
}