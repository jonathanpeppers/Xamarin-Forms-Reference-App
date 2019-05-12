using System;
using System.Collections.Generic;
using System.Linq;

using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Intune.Enrollment;
using Mobile.RefApp.Lib.Logging;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Intune.MAM;

namespace Mobile.RefApp.iOSLib.Intune.Enrollment
{
	public class EnrollmentService
	  : IntuneMAMEnrollmentDelegate,
		IEnrollmentService
	{
		private readonly ILoggingService _loggingService;

		public Action<Status, AuthenticationResult> EnrollmentRequestStatus { get; set; }

		public Action<Status> UnenrollmentRequestStatus { get; set; }

		public Action<Status> PolicyRequestStatus { get; set; }

		public bool IsIdentityManaged
			=> IntuneMAMPolicyManager.Instance.IsIdentityManaged(IntuneMAMEnrollmentManager.Instance.EnrolledAccount);

		public string EnrolledAccount
			=> IntuneMAMEnrollmentManager.Instance.EnrolledAccount;

		public List<string> RegisteredAccounts
			=> IntuneMAMEnrollmentManager.Instance.RegisteredAccounts.ToList();

		public Endpoint Endpoint { get; set; }

		public AuthenticationResult AuthenticationResults { get; set; }

		public EnrollmentService(ILoggingService loggingService)
		{
			_loggingService = loggingService;
			IntuneMAMEnrollmentManager.Instance.Delegate = this;
		}

		public void LoginAndEnrollAccount(
            string identity = null, 
            Endpoint endPoint = null, 
            IAzureAuthenticatorService authenticator = null)
		{
			try
			{
				if (endPoint != null)
					Endpoint = endPoint;

                if (Endpoint != null)
                    SetAdalInformation(endPoint);
                                        
                CacheToken token = AzureTokenCacheService.GetTokenByUpn(identity).FirstOrDefault();
                if (token != null)
                {
                    InvokeOnMainThread(() =>
                    {
                        //TODO 
                        //debugging - bug with getting tokens via register method
                        //working with @Kyle at Microsoft on issue.  Only work around
                        //is to run LoginAndEnroll instead

                        //IntuneMAMEnrollmentManager.Instance.LoginAndEnrollAccount(identity);

                        IntuneMAMEnrollmentManager.Instance.RegisterAndEnrollAccount(identity);
                    });
                }
                else
                {
                    IntuneMAMEnrollmentManager.Instance.LoginAndEnrollAccount(identity);
                }
			}
			catch (Exception ex)
			{
				_loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
			}
		}

		public void RegisterAndEnrollAccount(
            AuthenticationResult authenticationResult, 
            Endpoint endPoint = null)
		{
			try
			{
				if (endPoint != null)
					Endpoint = endPoint;

				if (Endpoint != null)
				{
                    SetAdalInformation(endPoint);
				}
                if (authenticationResult != null)
                    IntuneMAMEnrollmentManager.Instance.RegisterAndEnrollAccount(authenticationResult.UserInfo.DisplayableId);
                else
                    throw new Exception(Lib.Intune.Constants.Enrollment.ERRORNULL);

			}
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception ex)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning restore CS0168 // Variable is declared but never used
            {
#if DEBUG
                _loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
#endif
            }
		}

		public void DeRegisterAndUnenrollAccount(AuthenticationResult authenticationResult = null, bool withWipe = false)
		{
			try
			{
				IntuneMAMEnrollmentManager.Instance.DeRegisterAndUnenrollAccount(IntuneMAMEnrollmentManager.Instance.EnrolledAccount,
																				 withWipe);
			}
			catch (Exception ex)
			{
				_loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
			}
		}

        //set ADAL information via c# instead of info.plist file
        private void SetAdalInformation(Endpoint endpoint)
		{
			//IntuneMAMPolicyManager.Instance.AadClientIdOverride = adalClientId;
			//IntuneMAMPolicyManager.Instance.AadAuthorityUriOverride = adalAuthority;
			//IntuneMAMPolicyManager.Instance.AadRedirectUriOverride = adalRedirect;
		}

		public override void EnrollmentRequestWithStatus(IntuneMAMEnrollmentStatus status)
		{
			if (EnrollmentRequestStatus != null)
			{
				var es = new Status
				{
					DidSucceed = status.DidSucceed,
					Error = status.ErrorString,
					StatusCode = MapStatusCode(status.StatusCode)
				};
				EnrollmentRequestStatus(es, AuthenticationResults);
			}
		}

		public override void PolicyRequestWithStatus(IntuneMAMEnrollmentStatus status)
		{
			if (PolicyRequestStatus != null)
			{
				var es = new Status
				{
					DidSucceed = status.DidSucceed,
					Error = status.ErrorString,
					StatusCode = MapStatusCode(status.StatusCode)
				};
				PolicyRequestStatus(es);
			}
		}

		public override void UnenrollRequestWithStatus(IntuneMAMEnrollmentStatus status)
		{
			if (UnenrollmentRequestStatus != null)
			{
				var ues = new Status
				{
					DidSucceed = status.DidSucceed,
					Error = status.ErrorString,
					StatusCode = MapStatusCode(status.StatusCode)
				};
				UnenrollmentRequestStatus(ues);
			}
		}

		private StatusCode MapStatusCode(IntuneMAMEnrollmentStatusCode status)
		{
			switch (status)
			{
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusAccountNotLicensed:
					return StatusCode.AccountNotLicensed;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusADALInternalError:
					return StatusCode.ADALInternalError;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusAlreadyEnrolled:
					return StatusCode.AlreadyEnrolled;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusAppNotEnrolled:
					return StatusCode.AppNotEnrolled;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusAuthRequired:
					return StatusCode.AuthRequired;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusEnrollmentEndPointNetworkFailure:
					return StatusCode.EnrollmentEndPointNetworkFailure;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusFailedToClearMamData:
					return StatusCode.FailedToClearMamData;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusInternalError:
					return StatusCode.InternalError;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusLocationServiceFailure:
					return StatusCode.LocationServiceFailure;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusLoginCanceled:
					return StatusCode.LoginCanceled;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusMamServiceDisabled:
					return StatusCode.MamServiceDisabled;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusMdmEnrolledDifferentUser:
					return StatusCode.MdmEnrolledDifferentUser;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNewPoliciesReceived:
					return StatusCode.NewPoliciesReceived;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNilAccount:
					return StatusCode.NullAccount;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNoPolicyReceived:
					return StatusCode.NoPolicyReceived;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNotDeviceAccount:
					return StatusCode.NotDeviceAccount;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNotEmmAccount:
					return StatusCode.NotEmmAccount;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusNotEnrolledAccount:
					return StatusCode.AppNotEnrolled;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusParsingFailure:
					return StatusCode.ParsingFailure;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusPoliciesHaveNotChanged:
					return StatusCode.PoliciesHaveNotChanged;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusPolicyEndPointNetworkFailure:
					return StatusCode.PolicyEndPointNetworkFailure;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusPolicyRecordGone:
					return StatusCode.PolicyRecordGone;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusReEnrollForUnenrolledUser:
					return StatusCode.ReEnrollForUnenrolledUser;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusSwitchExistingAccount:
					return StatusCode.SwitchExistingAccount;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusTimeout:
					return StatusCode.Timeout;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusUnenrollmentSuccess:
					return StatusCode.UnenrollmentSuccess;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusUnsupportedAPI:
					return StatusCode.MAMUnsupportedAPI;
				case IntuneMAMEnrollmentStatusCode.MAMEnrollmentStatusWipeReceived:
					return StatusCode.WipeReceived;
			}
			return StatusCode.StatusUnknown;
		}

	}
}