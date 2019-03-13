using System;
using System.Data.Common;
using System.Data.Entity;

namespace Be.ManagedDataAccess.EntityFramework.Test
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext()
        {
        }

        public DbSet<LastWinsEntity> LastWins { get; set; }

        public DbSet<OptiEntity> Optis { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("EOX");
            base.OnModelCreating(modelBuilder);
        }
    }
}
