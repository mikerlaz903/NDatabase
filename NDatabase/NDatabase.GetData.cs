using System.Data;
using System.Data.Common;

namespace NDatabase
{
    public partial class NDatabase
    {
        private Func<
               DbDataReader,
               IEnumerable<dynamic>>? _fromSyncDelegateTable;
        public override IEnumerable<dynamic> GetTable(
            string sql, IEnumerable<object> parameterCollection)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var reader = resultCommand.ExecuteReader();

            if (reader is { } dbReader && _fromSyncDelegateTable != null)
                return _fromSyncDelegateTable(dbReader);

            throw new NotImplementedException("Reader cannot be executed.");
        }

        public override IEnumerable<dynamic> GetRow(
            string sql, IEnumerable<object> parameterCollection)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var reader = resultCommand.ExecuteReader();

            return GetDataRow(reader);
        }

        public override dynamic GetField(
            string sql, IEnumerable<object> parameterCollection)
        {
            _memoryStorage.Clear();

            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            resultCommand.Parameters.AddRange(parameterCollection.ToArray());
            var scalar = resultCommand.ExecuteScalar();

            if (scalar != null)
                return GetDataField(scalar);

            throw new NotImplementedException("Reader cannot be executed.");
        }

        public override DataTable? GetSchema(string sql)
        {
            if (_connection == null || _transaction == null)
                throw new NotImplementedException("Connection wasn't open.");

            var resultCommand = GetCommand(sql);
            var reader = resultCommand.ExecuteReader(CommandBehavior.SchemaOnly);

            return reader.GetSchemaTable();
        }
    }
}
