using SPO.ColdStorage.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class SharePointMigration : BaseDBObject
    {
        [Column("started")]
        public DateTime Started { get; set; }


        [Column("finished")]
        public DateTime Finished { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("storage_connection")]
        public string StorageAccount { get; set; } = string.Empty;

        public List<TargetSharePointSite> TargetSites { get; set; } = new();
    }
}
