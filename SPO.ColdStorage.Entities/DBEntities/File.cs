using SPO.ColdStorage.Entities.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.DBEntities
{
    [Table("files")]
    public class File : BaseDBObjectWithUrl
    {
        [ForeignKey(nameof(Web))]
        [Column("web_id")]
        public int WebId { get; set; }

        public Web Web { get; set; } = null!;

        [Column("last_modified")]
        public DateTime LastModified { get; set; } = DateTime.MinValue;

        [Column("last_modified_by")]
        public string LastModifiedBy { get; set; } = string.Empty;
    }
}
