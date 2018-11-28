using CredentialManagement;
using WebResourceManager.Models;

namespace WebResourceManager.Helpers
{
    public static class CredentialManager
    {
        public static string GetCredential(string target)
        {
            var cm = new Credential { Target = target };
            if (!cm.Load())
            {
                return null;
            }

            //UserPass is just a class with two string properties for user and pass
            return cm.Password;
        }

        public static bool SetCredentials(
             string target, string password, PersistanceType persistenceType)
        {
            return new Credential
            {
                Target = target,
                Password = password,
                PersistanceType = persistenceType
            }.Save();
        }

        public static bool RemoveCredentials(string target)
        {
            return new Credential { Target = target }.Delete();
        }
    }
}
