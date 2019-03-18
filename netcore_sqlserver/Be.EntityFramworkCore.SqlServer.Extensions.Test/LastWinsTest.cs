using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Be.EntityFrameworkCore.SqlServer.Test
{
    [TestClass]
    public class LastWins
    {
        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();
            return config;
        }

        public static SqlDbContext CreateContext()
        {
            var config = InitConfiguration();
            var cn = config.GetConnectionString("SqlDatabase");
            var builder = new DbContextOptionsBuilder<SqlDbContext>()
                .UseSqlServer(cn);

            var context = new SqlDbContext(builder.Options);
            return context;
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void DublicateKey()
        {
            var id = Guid.NewGuid();

            using (var cx = CreateContext())
            {
                var lw = new LastWinsEntity();
                lw.Id = id;
                lw.Name = "v1";
                lw.Created = lw.Updated = DateTime.Now;
                cx.LastWins.Add(lw);
                cx.SaveChanges();
            }
            
            using (var cx = CreateContext())
            {
                try
                {
                    var lw = new LastWinsEntity();
                    lw.Id = id;
                    lw.Name = "v2";
                    lw.Created = lw.Updated = DateTime.Now;
                    cx.LastWins.Add(lw);
                    cx.SaveChanges();
                }
                catch(DbUpdateException dex)
                {
                    var oex = dex.InnerException as SqlException;
                    if(oex?.Number == 2627)
                    {
                        throw new Exception("dublicate key", dex);
                    }
                }
            }
        }

        [TestMethod]
        public void DublicateKey_Context()
        {
            var id = Guid.NewGuid();
            var lwc = CreateOptiContext();

            lwc.SelectFunc = (cx) =>
            {
                var i = cx.LastWins.Find(id);
                return i;
            };

            lwc.AddAction = (cx) =>
            {
                var lw = new LastWinsEntity();
                lw.Id = id;
                lw.Name = "v1";
                lw.Created = lw.Updated = DateTime.Now;
                cx.LastWins.Add(lw);
                cx.SaveChanges();
            };
            lwc.UpdateAction = (cx, lw) =>
            {
                lw.Name = "v2";
                lw.Created = lw.Updated = DateTime.Now;
                cx.SaveChanges();
            };

            // begin select
            var cx2 = lwc.ContextFactory();
            var entity = lwc.SelectFunc(cx2);

            // add in the meantime
            lwc.Execute(); // first add

            if(entity == null) // entity is always null
                lwc.TryAdd(cx2); // simulate add with dublicate key
        }

        [TestMethod]
        public void DublicateKeyHandling_AddUpdate()
        {
            var id = Guid.NewGuid();

            var lwc = CreateOptiContext();
            lwc.SelectFunc = (cx) =>
                {
                    return cx.LastWins.FirstOrDefault(e => e.Id == id);
                };
            lwc.AddAction = (cx) =>
                {
                    var lw = new LastWinsEntity();
                    lw.Id = id;
                    lw.Name = "v2";
                    lw.Created = lw.Updated = DateTime.Now;
                    cx.LastWins.Add(lw);
                    cx.SaveChanges();
                };
            lwc.UpdateAction = (cx, lw) =>
                {
                    lw.Name = "v2";
                    lw.Updated = DateTime.Now;
                    cx.SaveChanges();
                };

            lwc.Execute(); // add
            lwc.Execute(); // update
        }

        public OptiContext<SqlDbContext, LastWinsEntity> CreateOptiContext()
        {
            var result = new OptiContext<SqlDbContext, LastWinsEntity>(() => CreateContext());
            return result;
        }

        [TestMethod]
        public void DublicateKeyHandling_AddDeleteAdd()
        {
            var id = Guid.NewGuid();
            var lwc = CreateOptiContext();

            lwc.SelectFunc = (cx) =>
            {
                var i = cx.LastWins.Find(id);
                return i;
            };

            lwc.AddAction = (cx) =>
            {
                var lw = new LastWinsEntity();
                lw.Id = id;
                lw.Name = "v1";
                lw.Created = lw.Updated = DateTime.Now;
                cx.LastWins.Add(lw);
                cx.SaveChanges();
            };
            lwc.UpdateAction = (cx, lw) =>
            {
                lw.Name = "v2";
                lw.Created = lw.Updated = DateTime.Now;
                cx.SaveChanges();
            };

            lwc.Execute(); // add

            Delete(id); // delete in the meantime

            lwc.Execute(); // add 2
        }

        [TestMethod]
        public void DublicateKeyHandling_AddSelectDeleteUpdate()
        {
            var id = Guid.NewGuid();
            var lwc = CreateOptiContext();

            lwc.SelectFunc = (cx) =>
            {
                var i = cx.LastWins.Find(id);
                return i;
            };

            lwc.AddAction = (cx) =>
            {
                var lw = new LastWinsEntity();
                lw.Id = id;
                lw.Name = "v1";
                lw.Created = lw.Updated = DateTime.Now;
                cx.LastWins.Add(lw);
                cx.SaveChanges();
            };
            lwc.UpdateAction = (cx, lw) =>
            {
                lw.Name = "v2";
                lw.Created = lw.Updated = DateTime.Now;
                cx.SaveChanges();
            };
            lwc.Execute();

            // simulate Execute part 1 -> create context and select entity
            var cx2 = lwc.ContextFactory();
            var entity = lwc.SelectFunc(cx2);

            // delete in the meantime
            Delete(id); 

            // simulate Execute part 2 -> TryUpdate the existing entity. Its not here, so AddAction is called
            lwc.TryUpdate(cx2, entity);
        }

        private void Delete(Guid id)
        {
            Task.Run(() =>
            {
                using (var cx = CreateContext())
                {
                    var lw = cx.LastWins.FirstOrDefault(e => e.Id == id);
                    cx.LastWins.Remove(lw);
                    cx.SaveChanges();
                }
            }).Wait();
        }
    }
}
