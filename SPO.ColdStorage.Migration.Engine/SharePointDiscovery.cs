using Azure.Identity;
using SPO.ColdStorage.Entities;

namespace SPO.ColdStorage.Migration.Engine
{
    public class SharePointDiscovery
    {
        private ClientSecretCredential _clientSecretCredential;
        private Config _config;

        public SharePointDiscovery(Config config)
        {
            _clientSecretCredential = new ClientSecretCredential(config.AzureAdConfig.TenantId, config.AzureAdConfig.ClientID, config.AzureAdConfig.Secret);
            _config = config;
        }

        public async Task StartAsync()
        {
            using (var db = new ColdStorageDbContext(this._config.SQLConnectionString))
            {

            }
        }
    }
}