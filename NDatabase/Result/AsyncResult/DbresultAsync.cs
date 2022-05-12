namespace NDatabase.Result.AsyncResult
{
    public partial class DbresultAsync<TEntity> : IAsyncEnumerable<TEntity>
    {
        private readonly IAsyncEnumerable<TEntity>? _asyncEnumerable;
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(
            CancellationToken cancellationToken = new())
        {
            if (_asyncEnumerable == null)
                throw new NotImplementedException();
            return new DbResultAsyncEnumerator<TEntity>(_asyncEnumerable.GetAsyncEnumerator(cancellationToken));
        }

        public DbresultAsync(IAsyncEnumerable<TEntity> asyncEnumerable)
        {
            _asyncEnumerable = asyncEnumerable;
        }
    }
}
