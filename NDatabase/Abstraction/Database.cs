using System.Data;
using System.Data.Common;

namespace NDatabase.Abstraction
{
    public abstract class Database : IAsyncDisposable, IDisposable
    {
        public abstract DbConnection? GetConnection();
        public abstract DbTransaction? GetTransaction();

        public abstract NDatabase FromMemory();
        public abstract NDatabase FromServer();
        public abstract NDatabase FromCombined();


        public abstract void Commit();
        public abstract void Rollback();

        public abstract void RollbackAndClose();

        public abstract DataTable? GetSchema(
            string sql);
        public abstract Task<DataTable?> GetSchemaAsync(
            string sql);
        public abstract Task<DataTable?> GetSchemaAsync(
            string sql, CancellationToken token);

        public abstract dynamic GetField(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<dynamic> GetFieldAsync(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<dynamic> GetFieldAsync(
            string sql, IEnumerable<object> parameterCollection,
            CancellationToken token);

        public abstract IEnumerable<dynamic> GetRow(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<IAsyncEnumerable<dynamic>> GetRowAsync(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<IAsyncEnumerable<dynamic>> GetRowAsync(
            string sql, IEnumerable<object> parameterCollection,
            CancellationToken token);

        public abstract IEnumerable<dynamic> GetTable(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<IAsyncEnumerable<dynamic>> GetTableAsync(
            string sql, IEnumerable<object> parameterCollection);
        public abstract Task<IAsyncEnumerable<dynamic>> GetTableAsync(
            string sql, IEnumerable<object> parameterCollection,
            CancellationToken token);

        public abstract int ExecuteNonQuery(
            string sql, IEnumerable<object> parameterCollection,
            bool autoCommit = true);
        public abstract Task<int> ExecuteNonQueryAsync(
            string sql, IEnumerable<object> parameterCollection,
            bool autoCommit = true);
        public abstract Task<int> ExecuteNonQueryAsync(
            string sql, IEnumerable<object> parameterCollection,
            CancellationToken token, bool autoCommit = true);

        public abstract List<int> ExecuteManyNonQuery(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            bool autoCommit = true);
        public abstract Task<List<int>> ExecuteManyNonQueryAsync(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            bool autoCommit = true);
        public abstract Task<List<int>> ExecuteManyNonQueryAsync(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            CancellationToken token, bool autoCommit = true);
        public abstract DataTable GetFilledTable(
            string sql, IEnumerable<object> parameterCollection,
            DataTable table, int startRecord = 0, int offset = 100);

        public abstract ValueTask DisposeAsync();
        public abstract void Dispose();
    }
}
