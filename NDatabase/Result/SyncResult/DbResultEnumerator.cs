using System.Collections;

namespace NDatabase.Result.SyncResult
{
    internal class DbResultEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private readonly IEnumerator<TEntity> _inner;

        public TEntity Current => _inner.Current;
        object? IEnumerator.Current => Current;

        public DbResultEnumerator(IEnumerator<TEntity> inner)
        {
            _inner = inner;
        }

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }



        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
