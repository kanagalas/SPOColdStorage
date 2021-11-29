using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Entities
{
    public class ColdStorageDbContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ColdStorageDbContext(string connectionString)
        { 
            this.ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
        public DbSet<SharePointMigration> Migrations { get; set; }
        public DbSet<TargetSharePointSite> TargetSharePointSites { get; set; }
        public DbSet<SharePointFile> Files { get; set; }
        public DbSet<MigrationError> MigrationErrors { get; set; }
        public DbSet<SuccesfulMigration> SuccesfulMigrations { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(this.ConnectionString);
    }
}
