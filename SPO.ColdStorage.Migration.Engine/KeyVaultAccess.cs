using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine
{
    internal class KeyVaultAccess
    {
        public static async Task<X509Certificate2> RetrieveCertificate(string name, string keyVaultUrl)
        {
            var serviceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));

            var secretUri = $"{keyVaultUrl}/Secrets/{name}";     // The Name of the secret / certificate      
            SecretBundle secret = await keyVaultClient.GetSecretAsync(secretUri);

            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(secret.Value));
            return certificate;

        }
    }
}
