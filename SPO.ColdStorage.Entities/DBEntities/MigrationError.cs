using SPO.ColdStorage.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.DBEntities
{
    public class MigrationError : BaseFileRelatedClass
    {
        [Column("error")]
        public string Error { get; set; } = string.Empty;   
    }
}
