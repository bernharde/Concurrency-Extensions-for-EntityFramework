namespace System.Data.Entity
{
    /// <summary>
    /// Specifiies the concurrency behaviour during SaveChanges method.
    /// </summary>
    public enum SaveChangesMode
    {
        /// <summary>
        /// Nothing is ignored (is the same as SaveChanges)
        /// </summary>
        None,
        /// <summary>
        /// Is a combination of every "Ignore" enum member
        /// </summary>
        All,
        /// <summary>
        /// Ignores if another user deleted the entity in the meantime
        /// </summary>
        IgnoreEntityDeleted,
        /// <summary>
        /// Ignores if another users added an entity entity with the same primary key in the meantime
        /// </summary>
        IgnoreEntityDublicateKey
    }
}
