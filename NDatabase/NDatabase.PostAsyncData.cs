namespace NDatabase
{
    public partial class NDatabase
    {
        public override Task<int> ExecuteNonQueryAsync(
            string sql, IEnumerable<object> parameterCollection, bool autoCommit = true)
        {
            return ExecuteNonQueryAsync(sql, parameterCollection, new CancellationToken(), autoCommit);
        }
        public override async Task<int> ExecuteNonQueryAsync(
            string sql, IEnumerable<object> parameterCollection,
            CancellationToken token, bool autoCommit = true)
        {
            if (_connection == null || _transaction == null) return 0;

            var command = GetCommand(sql);
            command.Parameters.AddRange(parameterCollection.ToArray());

            var rowEffected = await command.ExecuteNonQueryAsync(token);

            if (autoCommit && command.Transaction != null)
                await command.Transaction.CommitAsync(token);

            return rowEffected;
        }

        public override Task<List<int>> ExecuteManyNonQueryAsync(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            bool autoCommit = true)
        {
            return ExecuteManyNonQueryAsync(sql, parametersCollection, new CancellationToken(), autoCommit);
        }
        public override async Task<List<int>> ExecuteManyNonQueryAsync(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            CancellationToken token, bool autoCommit = true)
        {
            if (_connection == null || _transaction == null) return new List<int>();


            var rowEffected = new List<int>();

            var command = GetCommand(sql);
            await command.PrepareAsync(token);
            foreach (var paramCollection in parametersCollection)
            {
                command.Parameters.Clear();
                command.Parameters.AddRange(paramCollection.ToArray());

                rowEffected.Add(await command.ExecuteNonQueryAsync(token));
            }
            if (autoCommit && command.Transaction != null)
                await command.Transaction.CommitAsync(token);

            return rowEffected;
        }
    }
}
