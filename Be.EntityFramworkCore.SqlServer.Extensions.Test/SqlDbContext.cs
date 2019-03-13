using Microsoft.EntityFrameworkCore;

namespace Be.EntityFramworkCore.SqlServer.Test
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<LastWinsEntity> LastWins { get; set; }

        public DbSet<OptiEntity> Optis { get; set; }

    }
}
