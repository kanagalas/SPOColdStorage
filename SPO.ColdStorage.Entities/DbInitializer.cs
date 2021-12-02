using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Entities
{
    public class DbInitializer
    {

        /// <summary>
        /// Creates new tenant DB if needed
        /// </summary>
        /// <returns>If DB was created</returns>
        public async static Task<bool> Init(SPOColdStorageDbContext context, DevConfig config)
        {
            context.Database.EnsureCreated();
            if (context.Migrations.Any() || config == null)
            {
                return false;
            }

            // Add default data

            context.Migrations.Add(new SharePointMigration
            {
                Name = "Test migration",
                StorageAccount = config.DefaultStorageConnection,
                TargetSites = new List<TargetSharePointSite> { new TargetSharePointSite { RootURL = config.DefaultSharePointSite } }
            });
            await context.SaveChangesAsync();

            return true;
        }
    }
}
