using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Be.EntityFramwork.SqlServer.Test
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
