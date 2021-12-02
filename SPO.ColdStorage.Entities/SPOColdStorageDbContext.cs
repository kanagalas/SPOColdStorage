using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Entities
{
    public class SPOColdStorageDbContext : DbContext
    {
        public SPOColdStorageDbContext(string connectionString)
        { 
            this.ConnectionString = connectionString;
        }

        public SPOColdStorageDbContext(DbContextOptions<SPOColdStorageDbContext> options) : base(options)
        {
        }

        public string ConnectionString { get; set; } = String.Empty;


        public DbSet<SharePointMigration> Migrations { get; set; } = null!;
        public DbSet<TargetSharePointSite> TargetSharePointSites { get; set; } = null!;
        public DbSet<SharePointFile> Files { get; set; } = null!;
        public DbSet<MigrationError> MigrationErrors { get; set; } = null!;
        public DbSet<SuccesfulMigrationLog> SuccesfulMigrations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(this.ConnectionString);
    }
}
