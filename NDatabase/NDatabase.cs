using NDatabase.Abstraction;
using NDatabase.Enums;
using System.Data;
using System.Data.Common;

namespace NDatabase
{
    public sealed partial class NDatabase : Database
    {
        private DbConnection? _connection;
        private DbTransaction? _transaction;

        private readonly List<List<object>> _memoryStorage = new();
        private NDatabaseSource _source = NDatabaseSource.Default;
        private int MemoryBlockSize => 50;


        public NDatabase(DbConnection connection, IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            _connection = connection;
            _connection.Open();
            _transaction = _connection.BeginTransaction(isolation);
        }

        public NDatabase(DbConnection connection, NDatabaseSource source) : this(connection)
        {
            _source = source;
        }
        public override DbConnection? GetConnection()
            => _connection;
        public override DbTransaction? GetTransaction()
            => _transaction;

        /// <summary>
        /// Метод установки принципа получения данных.
        /// Получение данных полной выгрузкой в память.
        /// </summary>
        /// <returns>Класс с установленным признаком.</returns>
        public override NDatabase FromMemory()
        {
            _source = NDatabaseSource.FromMemory;
            SetFromDelegate();
            return this;
        }
        /// <summary>
        /// Метод установки принципа получения данных.
        /// Получение данных построчно с сервера.
        /// </summary>
        /// <returns>Класс с установленным признаком.</returns>
        public override NDatabase FromServer()
        {
            _source = NDatabaseSource.FromServer;
            SetFromDelegate();
            return this;
        }
        /// <summary>
        /// Метод установки принципа получения данных.
        /// Получение данных частичной выгрузкой их в память.
        /// </summary>
        /// <returns>Класс с установленным признаком.</returns>
        public override NDatabase FromCombined()
        {
            _source = NDatabaseSource.FromCombined;
            SetFromDelegate();
            return this;
        }

        private void SetFromDelegate()
        {
            var serverDelegateAsync = new Func<DbDataReader, CancellationToken, IAsyncEnumerable<object>>(GetDataFromServerTableAsync);
            var memoryDelegateAsync = new Func<DbDataReader, CancellationToken, IAsyncEnumerable<object>>(GetDataFromMemoryTableAsync);
            var combinedDelegateAsync = new Func<DbDataReader, CancellationToken, IAsyncEnumerable<object>>(GetDataFromCombinedTableAsync);
            _fromAsyncDelegateTable = _source switch
            {
                NDatabaseSource.FromMemory => memoryDelegateAsync,
                NDatabaseSource.FromServer => serverDelegateAsync,
                NDatabaseSource.Default => serverDelegateAsync,
                NDatabaseSource.FromCombined => combinedDelegateAsync,
                _ => throw new ArgumentOutOfRangeException()
            };

            var serverDelegate = new Func<DbDataReader, IEnumerable<object>>(GetDataFromServerTable);
            var memoryDelegate = new Func<DbDataReader, IEnumerable<object>>(GetDataFromMemoryTable);
            var combinedDelegate = new Func<DbDataReader, IEnumerable<object>>(GetDataFromCombinedTable);
            _fromSyncDelegateTable = _source switch
            {
                NDatabaseSource.FromMemory => memoryDelegate,
                NDatabaseSource.FromServer => serverDelegate,
                NDatabaseSource.Default => serverDelegate,
                NDatabaseSource.FromCombined => combinedDelegate,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private DbCommand GetCommand(string inputCommand)
        {
            if (_connection == null)
                throw new NotImplementedException("Connection wasn't open.");

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = inputCommand;

            return command;
        }

        /// <summary>
        /// Сохранение результатов в базу данных.
        /// </summary>
        public override void Commit()
        {
            if (_transaction?.Connection == null)
                return;

            _transaction.Commit();
            // поскольку транзакция читающая, 
            // мы её сразу заново запускаем
            _transaction = _connection?.BeginTransaction();
        }
        /// <summary>
        /// Отмена изменений.
        /// </summary>
        public override void Rollback() => _transaction?.Rollback();
        /// <summary>
        /// Отмена изменений и закрытие соединения.
        /// </summary>
        public override void RollbackAndClose()
        {
            if (_transaction?.Connection != null)
                _transaction.Rollback();
            _connection?.Close();
        }



        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Dispose();
                _transaction?.Dispose();
                //Result.Dispose();
            }

            _connection = null;
            _transaction = null;

            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_connection is not null)
                await _connection.DisposeAsync().ConfigureAwait(false);
            if (_transaction is not null)
                await _transaction.DisposeAsync().ConfigureAwait(false);


            _connection = null;
            _transaction = null;
        }
        ~NDatabase()
        {
            Dispose(false);
        }
    }
}
