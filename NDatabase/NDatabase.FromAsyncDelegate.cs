using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NDatabase
{
    public partial class NDatabase
    {
        /// <summary>
        /// Метод получения данных построчно с сервера.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="token"></param>
        /// <returns>Асинхронная коллекция данных с сервера.</returns>
        private async IAsyncEnumerable<object> GetDataFromServerTableAsync(DbDataReader reader, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var item in _memoryStorage)
                yield return item;

            if (reader.IsClosed || !reader.HasRows)
                yield break;

            var a = new dynamic[reader.FieldCount];
            while (await reader.ReadAsync(token))
            {
                reader.GetValues(a);
                var list = a.ToList();
                _memoryStorage.Add(list);
                yield return list;
            }

            if (!reader.HasRows)
                await reader.CloseAsync();
        }
        /// <summary>
        /// Метод получения данных из памяти с предварительной выгрузкой всех данных с сервера в память.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async IAsyncEnumerable<object> GetDataFromMemoryTableAsync(DbDataReader reader, [EnumeratorCancellation] CancellationToken token)
        {
            if (reader.IsClosed || !reader.HasRows)
            {
                await foreach (var resultRow in GetDataFromServerTableAsync(reader, token))
                {
                    if (resultRow is not List<object> result)
                        continue;
                    _memoryStorage.Add(result.ToList());
                }
            }

            foreach (var result in _memoryStorage)
            {
                yield return result;
            }
        }
        /// <summary>
        /// Метод получения данных из памяти с частичной выгрузкой данных с сервера.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async IAsyncEnumerable<object> GetDataFromCombinedTableAsync(DbDataReader reader, [EnumeratorCancellation] CancellationToken token)
        {
            foreach (var result in _memoryStorage)
            {
                yield return result;
            }
            while (!reader.IsClosed && reader.HasRows)
            {
                var count = 0;
                await foreach (var resultRow in GetDataFromServerTableAsync(reader, token))
                {
                    if (resultRow is not List<object> result)
                        continue;
                    _memoryStorage.Add(result.ToList());

                    if (++count >= MemoryBlockSize)
                        break;
                }
                foreach (var result in _memoryStorage)
                {
                    yield return result;
                }
            }
        }


        private async IAsyncEnumerable<List<dynamic>> GetAsyncDataRow(DbDataReader reader, [EnumeratorCancellation] CancellationToken token)
        {
            if (reader.IsClosed || !reader.HasRows)
                yield return _memoryStorage.First();

            var a = new dynamic[reader.FieldCount];
            if (await reader.ReadAsync(token))
            {
                reader.GetValues(a);
                var list = a.ToList();
                _memoryStorage.Add(list);
                yield return list;
            }
            if (!reader.IsClosed)
                await reader.CloseAsync();
        }


        private async IAsyncEnumerable<object> GetAsyncDataField(object scalar)
        {
            yield return scalar;
        }
    }
}
