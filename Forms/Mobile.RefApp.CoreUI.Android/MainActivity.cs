using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

using Xamarin.Forms;

using Unity;

using Mobile.RefApp.Lib.Logging;
using Mobile.RefApp.Lib.Network;
using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Intune;
using Mobile.RefApp.Lib.Intune.Enrollment;
using Mobile.RefApp.Lib.Intune.Policies;
using Mobile.RefApp.Droid.Network;
using Mobile.RefApp.DroidLib.ADAL;
using Mobile.RefApp.DroidLib.Intune;
using Mobile.RefApp.DroidLib.Network;
using Mobile.RefApp.DroidLib.Intune.Enrollment;
using Mobile.RefApp.DroidLib.Intune.Policies;

namespace Mobile.RefApp.CoreUI.Droid
{
    [Activity(Label = "Xam Ref App", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | 
        ConfigChanges.Orientation)]
    public class MainActivity 
        : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);

            LoadAuthenticaton();
            //initialize the app
            var refApp = new App();
            refApp.Init(PlatformInitializeContainer);
            LoadApplication(refApp);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //required for broker support with API > 22
        private void LoadAuthenticaton()
        {
            this.RequestPermissions(new string[] { Manifest.Permission.GetAccounts }, 0);
            this.RequestPermissions(new string[] { Manifest.Permission.ManageAccounts }, 1);
            this.RequestPermissions(new string[] { Manifest.Permission.UseCredentials }, 2);
        }

        public void PlatformInitializeContainer(UnityContainer container)
        {
            container.RegisterType<ILoggingService, LoggingService>();
            container.RegisterType<IPlatformHttpClientHandler, AndroidHttpClientHandler>();
            container.RegisterType<IAzurePlatformParameters, AzurePlatformParameters>();
            container.RegisterType<IIntuneService, IntuneService>();
            container.RegisterType<INetworkInterfaceInfo, NetworkInterfaceInfo>();
            container.RegisterType<IAzureAuthenticatorEndpointService, AzureAuthenticatorService>();

            container.RegisterType<IEnrollmentService, EnrollmentService>();
            container.RegisterType<IPolicyService, PolicyService>();
        }
    }
}