using System;
using System.Collections.Generic;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy.Notification;

using Mobile.RefApp.Lib.ADAL;
using Mobile.RefApp.Lib.Intune.Enrollment;
using Mobile.RefApp.Lib.Logging;

using Android.Runtime;

namespace Mobile.RefApp.DroidLib.Intune.Enrollment
{
    public class EnrollmentService : Java.Lang.Object,
        IEnrollmentService,
        IMAMNotificationReceiver 
    {
        private readonly ILoggingService _loggingService;
        private readonly IMAMEnrollmentManager _enrollmentManager;
        private readonly IMAMNotificationReceiverRegistry _notificationRegistery;
        private IMAMUserInfo _userInfo => MAMComponents.Get<IMAMUserInfo>();

        private Exception _registerError;
        private string _requestedIdentity;

        private readonly AuthenticationResult _authenticationResult;

        public Endpoint Endpoint { get; set; }

        public List<string> RegisteredAccounts { get; private set; }

        public Action<Status, AuthenticationResult> EnrollmentRequestStatus { get; set; }

        public Action<Status> UnenrollmentRequestStatus { get; set; }

        public Action<Status> PolicyRequestStatus { get; set; }

        public string EnrolledAccount => _userInfo?.PrimaryUser;

        public bool IsIdentityManaged => (_userInfo == null); 

        public EnrollmentService(ILoggingService loggingService)
		{
		    _loggingService = loggingService;
		    _enrollmentManager = MAMComponents.Get<IMAMEnrollmentManager>();
		    _notificationRegistery = MAMComponents.Get<IMAMNotificationReceiverRegistry>();

            _authenticationResult = null;
		    _registerError = null;
            Endpoint = null;
            RegisteredAccounts = new List<string>();

		    _notificationRegistery.RegisterReceiver(this, MAMNotificationType.MamEnrollmentResult);
		    _notificationRegistery.RegisterReceiver(this, MAMNotificationType.RefreshPolicy);
            _enrollmentManager.RegisterAuthenticationCallback(new MAMWEAuthCallback());
        }

        public void RegisterAndEnrollAccount(AuthenticationResult authenticationResult, Endpoint endPoint = null)
        {
            try
            {
                if (endPoint != null)
                    Endpoint = endPoint;
                else
                    throw new Exception(Lib.Intune.Constants.Enrollment.ERRORENDPOINTNULL);

                if (authenticationResult != null && Endpoint != null)
                {
                    var upn = authenticationResult?.UserInfo.DisplayableId;
                    var aadId = authenticationResult?.UserInfo?.UniqueId;
                    var tenantId = authenticationResult?.TenantId;

                    _loggingService.LogInformation(typeof(EnrollmentService), $"{Lib.Intune.Constants.Enrollment.ENROLLMENTLOGTAG} UPN {upn}\n TenantId: {tenantId}\n AadId: {aadId} \n");
                    _enrollmentManager.RegisterAccountForMAM(upn, aadId, tenantId);
                }
                else
                    throw new Exception(Lib.Intune.Constants.Enrollment.ERRORNULL);
            }
            catch (Exception ex)
            {
                var status = new Status
                {
                    Error = ex.Message,
                    DidSucceed = false,
                    StatusCode = StatusCode.InternalError
                };

                _loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
                EnrollmentRequestStatus(status, _authenticationResult);
            }
        }

        public void LoginAndEnrollAccount(string identity = null, 
            Endpoint endPoint = null, 
            IAzureAuthenticatorService authenticator = null)
        {
            try
            {
                _requestedIdentity = identity;
                //set the endpoint
                if (endPoint != null)
                    Endpoint = endPoint;

                CacheToken token = AzureTokenCacheService.GetTokenByUpn(identity)[0];
                if (token != null)
                {
                    var tenantId = token.TenantId;
                    var aadId = token.UserInfo?.UniqueId;
                    _enrollmentManager.RegisterAccountForMAM(identity, aadId, tenantId);
                    return;
                }
            }
            catch (Exception ex)
            {
                var status = new Status()
                {
                    Error = ex.Message,
                    DidSucceed = false,
                    StatusCode = StatusCode.InternalError
                };

                _loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
                EnrollmentRequestStatus(status, _authenticationResult);
            }
		}

		public void DeRegisterAndUnenrollAccount(AuthenticationResult authenticationResult, bool withWipe = false)
		{
			try
			{
            _enrollmentManager.UnregisterAccountForMAM(_authenticationResult.UserInfo.UniqueId);
			}
			catch (Exception ex)
			{
				_loggingService.LogError(typeof(EnrollmentService), ex, ex.Message);
			}
		}

		public new void Dispose()
		{
			if (_enrollmentManager != null)
				_enrollmentManager.Dispose();
		}

		public bool OnReceive(IMAMNotification notification)
		{
			var status = new Status();

			if (notification.Type == MAMNotificationType.MamEnrollmentResult)
			{
				var en = notification.JavaCast<IMAMEnrollmentNotification>();
				var result = en.EnrollmentResult;

				if (EnrollmentRequestStatus != null)
				{
					if (result == MAMEnrollmentManagerResult.AuthorizationNeeded)
					{
						status.StatusCode = StatusCode.AuthRequired;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.CompanyPortalRequired)
					{
						status.StatusCode = StatusCode.CompanyPortalRequired;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.EnrollmentFailed)
					{
						status.StatusCode = StatusCode.AppNotEnrolled;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.EnrollmentSucceeded)
					{
						status.StatusCode = StatusCode.EnrollmentSuccess;
						status.DidSucceed = true;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.MdmEnrolled)
					{
						status.StatusCode = StatusCode.MdmEnrolled;
						status.DidSucceed = true;
						status.Error = null;
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.NotLicensed)
					{
						status.StatusCode = StatusCode.AccountNotLicensed;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.WrongUser)
					{
						status.StatusCode = StatusCode.MdmEnrolledDifferentUser;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}

					EnrollmentRequestStatus(status, _authenticationResult);
				}
				else if (UnenrollmentRequestStatus != null)
				{

					if (result == MAMEnrollmentManagerResult.UnenrollmentFailed)
					{
						status.StatusCode = StatusCode.UnenrollmentFailed;
						status.DidSucceed = false;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}
					else if (result == MAMEnrollmentManagerResult.UnenrollmentSucceeded)
					{
						status.StatusCode = StatusCode.UnenrollmentSuccess;
						status.DidSucceed = true;
						status.Error = _registerError?.ToString();
						_registerError = null;
					}

					UnenrollmentRequestStatus(status);
				}
			}
			else if (notification.Type == MAMNotificationType.RefreshPolicy)
			{
				status.StatusCode = StatusCode.RefreshPolicy;
				status.DidSucceed = true;
				status.Error = null;
				PolicyRequestStatus(status);
			}
			return true;
		}

    }

    public class MAMWEAuthCallback 
        : Java.Lang.Object, 
          IMAMServiceAuthenticationCallback
    {
        public string AcquireToken(string upn, string aadId, string resourceId)
        {
            var result = string.Empty;
            var cacheTokens = AzureTokenCacheService.GetTokenByUpn(upn);
            foreach (var token in cacheTokens)
            {
                if (token.UserInfo?.UniqueId == aadId)
                {
                    return token.Token;
                }
            }
            return result;
        }
    }
}