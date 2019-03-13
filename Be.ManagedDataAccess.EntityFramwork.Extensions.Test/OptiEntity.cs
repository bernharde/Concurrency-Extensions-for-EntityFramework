using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Be.ManagedDataAccess.EntityFramework.Test
{
    [Table("XXCO")]
    public class OptiEntity
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Column("VALUE")]
        public int Value { get; set; }

        [Column("CREATED")]
        public DateTime Created { get; set; }

        [Column("UPDATED")]
        public DateTime Updated { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        public Guid RowVersion { get; set; }
    }
}
