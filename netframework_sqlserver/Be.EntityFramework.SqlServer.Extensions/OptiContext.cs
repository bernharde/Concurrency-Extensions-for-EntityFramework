using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace Be.EntityFramework.SqlServer
{
    /// <summary>
    /// Handling simple concurrency events with the last wins strategy.
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    /// <typeparam name="Entity"></typeparam>
    public class OptiContext<Context, Entity> 
        where Context : DbContext 
        where Entity : class 
    {
        public Func<Context> ContextFactory { get; set; }

        public OptiContext(Func<Context> contextFactory)
        {
            if (contextFactory == null)
                throw new ArgumentNullException($"{nameof(contextFactory)} can not be null");

            ContextFactory = contextFactory;
        }

        /// <summary>
        /// Function to retrieve a single entity, to decide whether add or update is required
        /// </summary>
        public Func<Context, Entity> SelectFunc { get; set; }
        /// <summary>
        /// Action to add the new entity. Executed, if SelectFunc returns no value!
        /// </summary>
        public Action<Context> AddAction { get; set; }
        /// <summary>
        /// Action to update the existing entity. Execute, if SelectFunc returns a value
        /// </summary>
        public Action<Context, Entity> UpdateAction { get; set; }

        /// <summary>
        /// Used SelectFunc, AddAction or UpdateAction as neccessary
        /// </summary>
        public void Execute()
        {
            if (SelectFunc == null)
                throw new ArgumentNullException($"{nameof(SelectFunc)} can not be null");
            if (AddAction == null)
                throw new ArgumentNullException($"{nameof(AddAction)} can not be null");
            if (UpdateAction == null)
                throw new ArgumentNullException($"{nameof(UpdateAction)} can not be null");

            int counter = 0;
            Execute_Internal(ref counter);
        }

        void Execute_Internal(ref int counter)
        {
            counter++;
            if(counter >= Extensions.MaxRetryCounter)
                throw new MaxRetriesReachedException();

            var cx = ContextFactory();
            var entity = SelectFunc(cx);
            if (entity == null)
                TryAdd_Internal(cx, ref counter);
            else
                TryUpdate_Internal(cx, entity, ref counter);
        }

        public void TryUpdate(Context cx, Entity entity)
        {
            int counter = 0;
            TryUpdate_Internal(cx, entity, ref counter);
        }

        void TryUpdate_Internal(Context cx, Entity entity, ref int counter)
        {
            try
            {
                UpdateAction(cx, entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                Execute_Internal(ref counter); // try again
            }
        }

        public void TryAdd(Context cx)
        {
            int counter = 0;
            TryAdd_Internal(cx, ref counter);
        }

        void TryAdd_Internal(Context cx, ref int counter)
        {
            try
            {
                AddAction(cx);
            }
            catch (DbUpdateException dex)
            {
                var oex = dex.InnerException?.InnerException as SqlException;
                if (oex?.Number == 2627)
                {
                    Execute_Internal(ref counter);
                            return;
                }
                throw dex;
            }
        }
    }
}
