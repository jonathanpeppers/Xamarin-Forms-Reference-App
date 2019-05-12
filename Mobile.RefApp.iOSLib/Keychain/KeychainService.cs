using System;
using System.Collections.Generic;

using Mobile.RefApp.Lib.Keychain;

using Security;

namespace Mobile.RefApp.iOSLib.Keychain
{
    public class KeychainService
        : IKeychainService

    {
        public ICollection<string> GetRecordsFromKeychain(string key)
        {
            List<string> returnResults = new List<string>(); 
            var queryRecord = new SecRecord(SecKind.GenericPassword)
            {
                AccessGroup = key
            };

            var records = SecKeyChain.QueryAsRecord(queryRecord, Int32.MaxValue, out SecStatusCode resultCode);
            if(resultCode == SecStatusCode.Success)
            {
                foreach (var r in records)
                {
                    var s = r.ValueData.ToString(Foundation.NSStringEncoding.UTF8);
                    returnResults.Add(s);
                }
            }
            return returnResults; 
        }

        public bool ClearRecordsFromKeychain(string key)
        {
            bool results = false;

            var queryRecord = new SecRecord(SecKind.GenericPassword)
            {
                AccessGroup = key
            };
            var queryResults = SecKeyChain.Remove(queryRecord);

            if (queryResults == SecStatusCode.Success)
                results = true;

            return results;
        }
    }
}
