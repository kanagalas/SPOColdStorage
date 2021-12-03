using SPO.ColdStorage.Entities.DBEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.Abstract
{
    /// <summary>
    /// Base database object
    /// </summary>
    public abstract class BaseDBObject
    {

        [Key]
        [Column("id")]
        public int ID { get; set; }


        public override string ToString()
        {
            return $"{this.GetType().Name} ID={ID}";
        }
    }

    public abstract class BaseFileRelatedClass : BaseDBObject
    {
        [ForeignKey(nameof(File))]
        [Column("file_id")]
        public int FileId { get; set; }

        [Required]
        public SharePointFile File { get; set; } = new SharePointFile();


        [ForeignKey(nameof(Migration))]
        [Column("migration_id")]
        public int MigrationId { get; set; }

        [Required]
        public SharePointMigration Migration { get; set; } = new SharePointMigration();
    }
}
