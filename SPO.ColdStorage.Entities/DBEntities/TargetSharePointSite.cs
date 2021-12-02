using SPO.ColdStorage.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class TargetSharePointSite : BaseDBObject
    {
        [Column("root_url")]
        public string RootURL { get; set; } = string.Empty;
    }
}
