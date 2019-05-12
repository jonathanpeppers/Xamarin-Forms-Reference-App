
namespace Mobile.RefApp.DroidLib.Intune.Policies
{
	public interface IAndroidPolicyService
	{
		//Android specific
		bool IsScreenCaptureAllowed { get; }
		bool IsSaveToPersonalAllowed { get; }
		bool IsFileEncrytionUsed { get; }
	}
}