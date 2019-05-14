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

        Action<Status> EnrollmentRequestStatus { get; set; }
		Action<Status> UnenrollmentRequestStatus { get; set; }
		Action<Status> PolicyRequestStatus { get; set; }

		void RegisterAndEnrollAccount(Endpoint endPoint);
		void LoginAndEnrollAccount(Endpoint endpoint, string identity = null);
		void DeRegisterAndUnenrollAccount(bool withWipe = false);


	}
}

