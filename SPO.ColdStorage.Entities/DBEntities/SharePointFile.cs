using SPO.ColdStorage.Entities.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class SharePointFile : BaseDBObject
    {
        [Required]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;
    }
}
