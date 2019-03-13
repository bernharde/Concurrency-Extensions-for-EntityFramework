using System;
using System.Data.Entity.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Be.ManagedDataAccess.EntityFramework.Test
{
    [TestClass]
    public class OptisTest
    {
        [TestMethod]
        public void AddOpti()
        {
            using (var cx = new OracleDbContext())
            {
                var opti = new OptiEntity();
                opti.Id = Guid.NewGuid();
                opti.Value = 0;
                opti.RowVersion = Guid.NewGuid();
                opti.Created = opti.Updated = DateTime.Now;
                cx.Optis.Add(opti);
                cx.SaveChanges();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DbUpdateConcurrencyException))]
        public void AddOptiWithUpdateConflict()
        {
            var id = Guid.NewGuid();
            using (var cx = new OracleDbContext())
            {
                var opti = new OptiEntity();
                opti.Id = id;
                opti.Value = 0;
                opti.RowVersion = Guid.NewGuid();
                opti.Created = opti.Updated = DateTime.Now;
                cx.Optis.Add(opti);
                cx.SaveChanges();

                ModifyOpti(id);

                opti.Value++;
                opti.Updated = DateTime.Now;
                opti.RowVersion = Guid.NewGuid();
                cx.SaveChanges();
            }
        }

        public OptiContext<OracleDbContext, OptiEntity> CreateOptiContext()
        {
            var result = new OptiContext<OracleDbContext, OptiEntity>(() => new OracleDbContext());
            return result;
        }

        [TestMethod]
        
        public void AddOptiWithUpdateConflictSolving()
        {
            var id = Guid.NewGuid();
            var initialRowVersion = Guid.NewGuid();
            var lastRowVersion = Guid.NewGuid();
            var oc = CreateOptiContext();

            var updateCounter = 0;

            oc.SelectFunc = (cx) =>
            {
                var i = cx.Optis.Find(id);
                return i;
            };

            oc.AddAction = (cx) =>
            { 
                var oe = new OptiEntity();
                oe.Id = id;
                oe.Value = 0;
                oe.Created = oe.Updated = DateTime.Now;
                oe.RowVersion = initialRowVersion;
                cx.Optis.Add(oe);
                cx.SaveChanges();
            };
            oc.UpdateAction = (cx, oe) =>
            {
                // test information
                updateCounter++;
                if(updateCounter == 2)
                    Assert.AreNotEqual(initialRowVersion, oe.RowVersion);

                // update the entity
                lastRowVersion = Guid.NewGuid();
                oe.Value++;
                oe.Created = oe.Updated = DateTime.Now;
                oe.RowVersion = lastRowVersion;
                cx.SaveChanges();
            };

            // initial add opti entity
            oc.Execute();

            // simulate Execute part 1 -> create context and select entity
            var cx2 = oc.ContextFactory();
            var entity = oc.SelectFunc(cx2);

            // modify in the meantime in another dbcontext
            ModifyOpti(id);

            // simulate Execute part 2 -> TryUpdate the existing entity. Its  here, but modified
            oc.TryUpdate(cx2, entity);

            // update must be called twice!
            Assert.AreEqual(2, updateCounter);

            // check if the last row version is stored in the database
            using (var cx = new OracleDbContext())
            {
                var opti = cx.Optis.Find(id);
                Assert.AreEqual(lastRowVersion, opti.RowVersion);
            }
        }

        private void ModifyOpti(Guid id)
        {
            using (var cx = new OracleDbContext())
            {
                var opti = cx.Optis.Find(id);
                opti.Value++;
                opti.Updated = DateTime.Now;
                opti.RowVersion = Guid.NewGuid();
                cx.SaveChanges();
            }
        }
    }
}
