using System;
using System.Collections.Generic;

namespace Mobile.RefApp.Lib.Keychain
{
    public interface IKeychainService
    {
        ICollection<string> GetRecordsFromKeychain(string key);
        bool ClearRecordsFromKeychain(string key);
    }
}
