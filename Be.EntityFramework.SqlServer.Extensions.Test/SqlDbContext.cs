using System.Data.Entity;

namespace Be.EntityFramwork.SqlServer.Test
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext() : base("name=SqlDatabase")
        {
        }

        public DbSet<LastWinsEntity> LastWins { get; set; }

        public DbSet<OptiEntity> Optis { get; set; }

    }
}
