using NDatabase.Abstraction;
using System.Data.Common;

namespace NDatabase.Factories
{
    public sealed class NDatabaseCreator : Creator
    {
        public override Database GetDatabase(DbConnection connection)
        {
            return new NDatabase(connection);
        }
    }
}
