using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities.Configuration;

namespace SPO.ColdStorage.Entities
{
    public class SPOColdStorageDbContext : DbContext
    {
        private readonly Config _config;

        public SPOColdStorageDbContext(Config config)
        { 
            this._config = config;   
        }

        public SPOColdStorageDbContext(DbContextOptions<SPOColdStorageDbContext> options, Config config) : base(options)
        {
            this._config = config;
        }

        public DbSet<DBEntities.TargetMigrationSite> TargetSharePointSites { get; set; } = null!;
        public DbSet<DBEntities.Site> Sites { get; set; } = null!;
        public DbSet<DBEntities.Web> Webs { get; set; } = null!;
        public DbSet<DBEntities.File> Files { get; set; } = null!;
        public DbSet<DBEntities.FileMigrationErrorLog> FileMigrationErrors { get; set; } = null!;
        public DbSet<DBEntities.FileMigrationCompletedLog> FileMigrationsCompleted { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(_config.ConnectionStrings.SQLConnectionString);
    }
}
