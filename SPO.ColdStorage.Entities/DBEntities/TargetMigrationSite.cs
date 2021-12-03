using SPO.ColdStorage.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.DBEntities
{
    [Table("target_migration_sites")]
    public class TargetMigrationSite : BaseDBObject
    {
        [Column("root_url")]
        public string RootURL { get; set; } = string.Empty;
    }
}
