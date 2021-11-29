using SPO.ColdStorage.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class TargetSharePointSite : BaseDBObject
    {
        [Column("graph_site_id")]
        public string GraphSiteId { get; set; } = string.Empty;
    }
}
