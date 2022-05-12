using System.Collections;

namespace NDatabase.Result.SyncResult
{
    public partial class DbResult<TEntity> : IEnumerable<TEntity>
    {
        private readonly IEnumerable<TEntity>? _enumerable;
        public IEnumerator<TEntity> GetEnumerator()
        {
            if (_enumerable == null)
                throw new NotImplementedException();
            return new DbResultEnumerator<TEntity>(_enumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public DbResult(IEnumerable<TEntity> enumerable)
        {
            _enumerable = enumerable;
        }
    }
}
