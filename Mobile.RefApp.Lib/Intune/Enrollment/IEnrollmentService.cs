using Mobile.RefApp.Lib.ADAL;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using System;
using System.Collections.Generic;

namespace Mobile.RefApp.Lib.Intune.Enrollment
{
	public interface IEnrollmentService
	{
		Endpoint Endpoint { get; set; }
		string EnrolledAccount { get; }
		List<string> RegisteredAccounts { get; }
        bool IsIdentityManaged { get; }

        Action<Status, AuthenticationResult> EnrollmentRequestStatus { get; set; }
		Action<Status> UnenrollmentRequestStatus { get; set; }
		Action<Status> PolicyRequestStatus { get; set; }

		void RegisterAndEnrollAccount(AuthenticationResult authenticationResult, Endpoint endPoint = null);

		void LoginAndEnrollAccount(string identity = null, Endpoint endpoint = null, IAzureAuthenticatorService authenticator = null);

		void DeRegisterAndUnenrollAccount(AuthenticationResult authenticationResult = null, bool withWipe = false);


	}
}

