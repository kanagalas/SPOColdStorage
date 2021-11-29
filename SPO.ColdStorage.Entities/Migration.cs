using SPO.ColdStorage.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities
{
    public class Migration : BaseDBObject
    {
        [Column("started")]
        public DateTime Started { get; set; }


        [Column("finished")]
        public DateTime Finished { get; set; }
    }
}
