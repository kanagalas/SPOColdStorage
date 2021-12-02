using SPO.ColdStorage.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class SuccesfulMigrationLog : BaseFileRelatedClass
    {
        [Column("migrated")]
        public DateTime Migrated { get; set; } = DateTime.MinValue;

        [Column("last_modified")]
        public DateTime LastModified { get; set; } = DateTime.MinValue;

    }
}
