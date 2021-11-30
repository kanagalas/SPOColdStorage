using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Services.AppAuthentication;
using SPO.ColdStorage.Entities;
using System.Security.Cryptography.X509Certificates;

namespace SPO.ColdStorage.Migration.Engine
{
    internal class KeyVaultAccess
    {
        public static async Task<X509Certificate2> RetrieveCertificate(string name, Config config)
        {
            var client = new SecretClient(vaultUri: new Uri(config.KeyVaultUrl), credential: new ClientSecretCredential(config.AzureAdConfig.TenantId, config.AzureAdConfig.ClientID, config.AzureAdConfig.Secret));


            var secretUri = $"{config.KeyVaultUrl}/Secrets/{name}";     // The Name of the secret / certificate      
            var secret = await client.GetSecretAsync(name);

            var certificate = new X509Certificate2(Convert.FromBase64String(secret.Value.Value));
            return certificate;

        }
    }
}
