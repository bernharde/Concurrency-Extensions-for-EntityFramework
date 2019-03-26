using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace System.Data.Entity
{
    public static class Extensions
    {
        /// <summary>
        /// (Default is 20) Gets or sets the counter SaveChanges is retried
        /// </summary>
        public static int MaxRetryCounter { get; set; } = 20;

        /// <summary>
        /// SaveChanges supporting modes to deal with concurrency
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        /// <param name="mode">mode to specifiy concurrency behavior</param>
        public static void SaveChanges(this DbContext dbContext, SaveChangesMode mode)
        {
            int counter = 0;
            SaveChanges_Internal(dbContext, mode, ref counter);
        }

        static void SaveChanges_Internal(DbContext dbContext, SaveChangesMode mode, ref int counter)
        {
            var redoSaveChanges = false;

            if (counter >= MaxRetryCounter)
                return; 

            try
            {
                dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException cex)
            {
                if (mode == SaveChangesMode.All || mode == SaveChangesMode.IgnoreEntityDeleted)
                {
                    foreach (var entiy in cex.Entries)
                    {
                        entiy.State = EntityState.Detached;
                    }
                    redoSaveChanges = true;
                }
                if(!redoSaveChanges)
                    throw cex;
            }
            catch (DbUpdateException dex)
            {
                var oex = dex.InnerException as SqlException;
                if (oex?.Number == 2627)
                {
                    if (mode == SaveChangesMode.All || mode == SaveChangesMode.IgnoreEntityDublicateKey)
                    {
                        foreach (var entry in dex.Entries)
                        {
                            entry.State = EntityState.Detached;
                        }
                        redoSaveChanges = true;
                    }
                }

                if (!redoSaveChanges)
                    throw dex;
            }

            if (redoSaveChanges)
            {
                counter++;
                SaveChanges_Internal(dbContext, mode, ref counter);
            }
        }
    }
}
