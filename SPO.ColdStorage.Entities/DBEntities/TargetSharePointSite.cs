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
        [Column("root_url")]
        public string RootURL { get; set; } = string.Empty;
    }
}
