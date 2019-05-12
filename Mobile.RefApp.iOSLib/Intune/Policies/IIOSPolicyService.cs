
using Mobile.RefApp.Lib.Intune.Policies;

namespace Mobile.RefApp.iOSLib.Intune.Policies
{
	public interface IIOSPolicyService
	{
		//iOS specific
		Source PolicySource { get; }
		string[] IntuneLogPaths { get; }
		bool ShouldFileProviderEncryptFiles { get; }
		bool IsAppSharingAllowed { get; }
		bool SpotlightIndexingAllowed { get; }
		bool IsUniversalLinkAllowed(string url);
		bool IsDocumentPickerAllowed(DocumentPickerMode documentPickerMode);
		bool TelemetryEnabled { get; }
		string PrimaryUser { get; }
		string ProcessIdentity { get; }
		string UIPolicyIdentity { get; }



    }
}