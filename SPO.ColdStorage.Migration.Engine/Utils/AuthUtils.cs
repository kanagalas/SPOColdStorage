using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace SPO.ColdStorage.Migration.Engine
{
    public class AuthUtils
    {
        static async Task<X509Certificate2> RetrieveKeyVaultCertificate(string name, string tenantId, string clientId, string clientSecret, string keyVaultUrl)
        {
            var client = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new ClientSecretCredential(tenantId, clientId, clientSecret));

            var secret = await client.GetSecretAsync(name);

            var certificate = new X509Certificate2(Convert.FromBase64String(secret.Value.Value));
            return certificate;

        }

        public async static Task<ClientContext> GetClientContext(string siteUrl, string tenantId, string clientId, string clientSecret, string keyVaultUrl, string baseServerAddress)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                throw new ArgumentException($"'{nameof(siteUrl)}' cannot be null or empty.", nameof(siteUrl));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or empty.", nameof(tenantId));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException($"'{nameof(clientId)}' cannot be null or empty.", nameof(clientId));
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or empty.", nameof(clientSecret));
            }

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentException($"'{nameof(keyVaultUrl)}' cannot be null or empty.", nameof(keyVaultUrl));
            }

            if (string.IsNullOrEmpty(baseServerAddress))
            {
                throw new ArgumentException($"'{nameof(baseServerAddress)}' cannot be null or empty.", nameof(baseServerAddress));
            }

            var appRegistrationCert = await AuthUtils.RetrieveKeyVaultCertificate("AzureAutomationSPOAccess", tenantId, clientId, clientSecret, keyVaultUrl);
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                  .WithCertificate(appRegistrationCert)
                                                  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                  .Build();
            var scopes = new string[] { $"{baseServerAddress}/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();


            var ctx = new ClientContext(siteUrl);
            ctx.ExecutingWebRequest += (s, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
            };

            return ctx;
        }

        public static async Task<ClientContext> GetClientContext(Config config, string siteUrl)
        {
            return await GetClientContext(siteUrl, config.AzureAdConfig.TenantId!, config.AzureAdConfig.ClientID!,
                config.AzureAdConfig.Secret!, config.KeyVaultUrl, config.BaseServerAddress);
        }
    }
}
