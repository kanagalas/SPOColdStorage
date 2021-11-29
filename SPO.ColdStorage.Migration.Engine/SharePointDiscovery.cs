using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
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
            var c = new GraphServiceClient(this._clientSecretCredential);

            _tracer.TrackTrace($"Migrating site ID '{siteId}'...");
            
            var crawler = new SiteCrawler(c, siteId, db, _tracer);
            await crawler.Start();
        }
    }
}
