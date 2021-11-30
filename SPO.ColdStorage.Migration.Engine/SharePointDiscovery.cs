using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;

namespace SPO.ColdStorage.Migration.Engine
{
    public class SharePointDiscovery
    {
        private ClientSecretCredential _clientSecretCredential;
        private Config _config;
        private DebugTracer _tracer;

        public SharePointDiscovery(Config config)
        {
            _clientSecretCredential = new ClientSecretCredential(config.AzureAdConfig.TenantId, config.AzureAdConfig.ClientID, config.AzureAdConfig.Secret);
            _config = config;
            _tracer = DebugTracer.ConsoleOnlyTracer();
        }

        public async Task StartAsync()
        {
            using (var db = new ColdStorageDbContext(this._config.SQLConnectionString))
            {
                var migrations = await db.Migrations.Include(m=> m.TargetSites).ToListAsync();
                foreach (var m in migrations)
                {
                    if (!m.Started.HasValue)
                    {
                        await StartMigration(m, db);
                    }
                }
            }
        }

        async Task StartMigration(Entities.DBEntities.SharePointMigration m, ColdStorageDbContext db)
        {
            foreach (var siteId in m.TargetSites)
            {
                await StartMigration(siteId.GraphSiteId, db);
            }
        }
        async Task StartMigration(string siteId, ColdStorageDbContext db)
        {
            var cert2 = await KeyVaultAccess.RetrieveCertificate("AzureAutomationSPOAccess", _config);
            var app = ConfidentialClientApplicationBuilder.Create(_config.AzureAdConfig.ClientID)
                                                  .WithCertificate(cert2)
                                                  .WithAuthority($"https://login.microsoftonline.com/{_config.AzureAdConfig.TenantId}")
                                                  .Build();
            var scopes = new string[] { $"https://m365x352268.sharepoint.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();


            var ctx = new ClientContext("https://m365x352268.sharepoint.com/sites/MigrationHost");
            ctx.ExecutingWebRequest += (s, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
            };


            _tracer.TrackTrace($"Migrating site ID '{siteId}'...");
            
            var crawler = new SiteCrawler(ctx, siteId, db, _tracer);
            await crawler.Start();
        }
    }
}
