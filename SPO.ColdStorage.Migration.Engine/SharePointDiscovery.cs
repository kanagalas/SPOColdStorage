using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;

namespace SPO.ColdStorage.Migration.Engine
{
    public class SharePointDiscovery
    {
        private Config _config;
        private DebugTracer _tracer;

        public SharePointDiscovery(Config config)
        {
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
                await StartMigration(siteId.RootURL, db);
            }
        }
        async Task StartMigration(string siteUrl, ColdStorageDbContext db)
        {
            var appRegistrationCert = await KeyVaultAccess.RetrieveCertificate("AzureAutomationSPOAccess", _config);
            var app = ConfidentialClientApplicationBuilder.Create(_config.AzureAdConfig.ClientID)
                                                  .WithCertificate(appRegistrationCert)
                                                  .WithAuthority($"https://login.microsoftonline.com/{_config.AzureAdConfig.TenantId}")
                                                  .Build();
            var scopes = new string[] { $"{_config.BaseServerAddress}/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();


            var ctx = new ClientContext(siteUrl);
            ctx.ExecutingWebRequest += (s, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
            };


            _tracer.TrackTrace($"Migrating site '{siteUrl}'...");
            
            var crawler = new SiteCrawler(ctx, _tracer);
            crawler.SharePointFileFound += Crawler_SharePointFileFound;
            await crawler.Start();
        }

        private void Crawler_SharePointFileFound(object? sender, Model.SharePointFileInfoEventArgs e)
        {
            _tracer.TrackTrace($"+file '{_config.BaseServerAddress + e.SharePointFileInfo.Url}'...");
        }
    }
}
