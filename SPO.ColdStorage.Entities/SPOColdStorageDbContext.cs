using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Entities
{
    public class SPOColdStorageDbContext : DbContext
    {
        public SPOColdStorageDbContext(string connectionString, Config config)
        { 
            this.connectionString = connectionString;
            this.config = config;   
        }

        public SPOColdStorageDbContext(DbContextOptions<SPOColdStorageDbContext> options, Config config) : base(options)
        {
            this.config = config;
        }

        private string connectionString = String.Empty;
        private readonly Config config;

        public DbSet<SharePointMigration> Migrations { get; set; } = null!;
        public DbSet<TargetSharePointSite> TargetSharePointSites { get; set; } = null!;
        public DbSet<SharePointFile> Files { get; set; } = null!;
        public DbSet<MigrationError> MigrationErrors { get; set; } = null!;
        public DbSet<SuccesfulMigrationLog> SuccesfulMigrations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(!String.IsNullOrEmpty(this.connectionString) ? this.connectionString : config.ConnectionStrings.SQLConnectionString);
    }
}
