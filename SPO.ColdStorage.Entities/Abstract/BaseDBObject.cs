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

    public abstract class BaseDBObjectWithName : BaseDBObject
    {
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

    }

    public abstract class BaseFileRelatedClass : BaseDBObject
    {
        [ForeignKey(nameof(File))]
        [Column("file_id")]
        public int FileId { get; set; }

        [Required]
        public DBEntities.File File { get; set; } = new DBEntities.File();


    }
}
