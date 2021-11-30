using SPO.ColdStorage.Entities.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities
{
    public class DbInitializer
    {

        /// <summary>
        /// Creates new tenant DB if needed
        /// </summary>
        /// <returns>If DB was created</returns>
        public async static Task<bool> Init(ColdStorageDbContext context, DevConfig config)
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
