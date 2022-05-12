using NDatabase.Abstraction;
using System.Data.Common;

namespace NDatabase.Factories
{
    public abstract class Creator
    {
        public abstract Database GetDatabase(DbConnection connection);
    }
}
