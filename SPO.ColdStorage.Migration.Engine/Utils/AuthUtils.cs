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
        public static async Task<X509Certificate2> RetrieveKeyVaultCertificate(string name, Config config)
        {
            var client = new SecretClient(vaultUri: new Uri(config.KeyVaultUrl), credential: new ClientSecretCredential(config.AzureAdConfig.TenantId, config.AzureAdConfig.ClientID, config.AzureAdConfig.Secret));

            var secret = await client.GetSecretAsync(name);

            var certificate = new X509Certificate2(Convert.FromBase64String(secret.Value.Value));
            return certificate;

        }

        public async static Task<ClientContext> GetClientContext(Config config, string siteUrl)
        {
            var appRegistrationCert = await AuthUtils.RetrieveKeyVaultCertificate("AzureAutomationSPOAccess", config);
            var app = ConfidentialClientApplicationBuilder.Create(config.AzureAdConfig.ClientID)
                                                  .WithCertificate(appRegistrationCert)
                                                  .WithAuthority($"https://login.microsoftonline.com/{config.AzureAdConfig.TenantId}")
                                                  .Build();
            var scopes = new string[] { $"{config.BaseServerAddress}/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();


            var ctx = new ClientContext(siteUrl);
            ctx.ExecutingWebRequest += (s, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
            };

            return ctx;
        }
    }
}
