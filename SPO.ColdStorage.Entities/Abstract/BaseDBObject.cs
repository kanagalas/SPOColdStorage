﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.Abstract
{
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

    public abstract class BaseFileRelatedClass : BaseDBObject
    {
        [ForeignKey(nameof(File))]
        [Column("file_id")]
        public int FileId { get; set; }

        [Required]
        public File File { get; set; } = new File();


        [ForeignKey(nameof(Migration))]
        [Column("migration_id")]
        public int MigrationId { get; set; }

        [Required]
        public Migration Migration { get; set; } = new Migration();
    }
}
