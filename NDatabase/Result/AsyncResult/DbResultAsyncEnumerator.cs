namespace NDatabase.Result.AsyncResult
{
    internal class DbResultAsyncEnumerator<TEntity> : IAsyncEnumerator<TEntity>
    {
        private readonly IAsyncEnumerator<TEntity> _inner;

        public TEntity Current => _inner.Current;
        public DbResultAsyncEnumerator(IAsyncEnumerator<TEntity> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return _inner.MoveNextAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await _inner.DisposeAsync();
        }
    }
}
