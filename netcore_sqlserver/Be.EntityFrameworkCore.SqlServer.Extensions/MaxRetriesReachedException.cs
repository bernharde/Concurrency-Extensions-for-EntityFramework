using System;

namespace Be.EntityFrameworkCore.SqlServer
{
    public class MaxRetriesReachedException : Exception
    {
        public MaxRetriesReachedException() { }
    }
}
