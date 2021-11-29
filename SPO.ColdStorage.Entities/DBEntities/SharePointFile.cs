using SPO.ColdStorage.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class SharePointFile : BaseDBObject
    {
        [Required]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;
    }
}
