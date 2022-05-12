using System.Data;
using System.Data.Common;

namespace NDatabase
{
    public partial class NDatabase
    {
        private Func<
            DbDataReader,
            CancellationToken,
            IAsyncEnumerable<dynamic>>? _fromAsyncDelegateTable;

        private async Task<DbCommand> GetCommandAsync(string inputCommand)
        {
            if (_connection == null)
                throw new NotImplementedException("Connection wasn't open.");

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
                _transaction = await _connection.BeginTransactionAsync();
            }

            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = inputCommand;

            return command;
        }

        public override Task<IAsyncEnumerable<dynamic>> GetTableAsync(
            string sql, IEnumerable<object> parameterCollection)
        {
            return GetTableAsync(sql, parameterCollection, new CancellationToken());
        }
        public override async Task<IAsyncEnumerable<dynamic>> GetTableAsync(
            string sql, IEnumerable<object> parameterCollection, CancellationToken token)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null) 
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var reader = await resultCommand.ExecuteReaderAsync(token);

            if (reader is { } dbReader && _fromAsyncDelegateTable != null)
                return _fromAsyncDelegateTable(dbReader, token);

            throw new NotImplementedException("Reader cannot be executed.");
        }

        public override Task<IAsyncEnumerable<dynamic>> GetRowAsync(
            string sql, IEnumerable<object> parameterCollection)
        {
            return GetRowAsync(sql, parameterCollection, new CancellationToken());
        }

        public override async Task<IAsyncEnumerable<dynamic>> GetRowAsync(
            string sql, IEnumerable<object> parameterCollection, CancellationToken token)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var reader = await resultCommand.ExecuteReaderAsync(token);

            if (reader is { } dbReader)
                return GetAsyncDataRow(reader, token);

            throw new NotImplementedException("Reader cannot be executed.");
        }


        public override Task<dynamic> GetFieldAsync(
            string sql, IEnumerable<object> parameterCollection)
        {
            return GetFieldAsync(sql, parameterCollection, new CancellationToken());
        }
        public override async Task<dynamic> GetFieldAsync(
            string sql, IEnumerable<object> parameterCollection, CancellationToken token)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var scalar = await resultCommand.ExecuteScalarAsync(token);

            if (scalar != null)
                return GetAsyncDataField(scalar);

            throw new NotImplementedException("Reader cannot be executed.");
        }



        public override DataTable GetFilledTable(
            string sql, IEnumerable<object> parameterCollection,
            DataTable table,
            int startRecord = 0, int offset = 100)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null) return table;

            var providerFactory = DbProviderFactories.GetFactory(_connection);

            var command = GetCommand(sql);
            command.Parameters.AddRange(parameterCollection.ToArray());

            var adapter = providerFactory?.CreateDataAdapter();
            if (adapter != null)
                adapter.SelectCommand = command;

            table.Clear();
            if (adapter != null)
                FillTableData(ref table, adapter, startRecord, offset);

            return table;
        }

        private void FillTableData(
            ref DataTable table, DbDataAdapter adapter,
            int startRecord = 0, int offset = 100)
        {
            var dataSet = new DataSet();

            if (table.Columns.Count <= 0)
                adapter.FillSchema(table, SchemaType.Mapped);
            adapter.Fill(dataSet, startRecord, offset, "test");

            if (dataSet.Tables["test"] is not { } filledTable)
                return;

            foreach (DataRow row in filledTable.Rows)
            {
                var newRow = table.NewRow();
                foreach (DataColumn column in filledTable.Columns)
                {
                    if (table.Columns.Contains(column.ColumnName))
                    {
                        newRow[column.ColumnName] = row.Field<object>(column) ?? DBNull.Value;
                    }
                }
                table.Rows.Add(newRow);
            }
        }

        public override Task<DataTable?> GetSchemaAsync(string sql)
        {
            return GetSchemaAsync(sql, new CancellationToken());
        }
        public override async Task<DataTable?> GetSchemaAsync(
            string sql, CancellationToken token)
        {
            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            var reader = await resultCommand.ExecuteReaderAsync(CommandBehavior.SchemaOnly, token);

            return await reader.GetSchemaTableAsync(token);
        }
    }
}
