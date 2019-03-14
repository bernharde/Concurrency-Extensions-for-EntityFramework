using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Be.EntityFramwork.SqlServer.Test
{
    [Table("XXCL")]
    public class LastWinsEntity
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("CREATED")]
        public DateTime Created { get; set; }

        [Column("UPDATED")]
        public DateTime Updated { get; set; }
    }
}
