using System.Data.Common;

namespace NDatabase
{
    public partial class NDatabase
    {
        private IEnumerable<object> GetDataFromServerTable(DbDataReader reader)
        {
            foreach (var item in _memoryStorage)
                yield return item;

            if (reader.IsClosed || !reader.HasRows)
                yield break;

            var a = new dynamic[reader.FieldCount];
            while (reader.Read())
            {
                reader.GetValues(a);
                var list = a.ToList();
                _memoryStorage.Add(list);
                yield return list;
            }

            if (!reader.HasRows)
                reader.Close();
        }
        private IEnumerable<object> GetDataFromMemoryTable(DbDataReader reader)
        {
            if (reader.IsClosed || !reader.HasRows)
            {
                foreach (var resultRow in GetDataFromServerTable(reader))
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
        private IEnumerable<object> GetDataFromCombinedTable(DbDataReader reader)
        {
            foreach (var result in _memoryStorage)
            {
                yield return result;
            }
            while (!reader.IsClosed && reader.HasRows)
            {
                var count = 0;
                foreach (var resultRow in GetDataFromServerTable(reader))
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

        private IEnumerable<List<dynamic>> GetDataRow(DbDataReader reader)
        {
            if (reader.IsClosed || !reader.HasRows)
                yield return _memoryStorage.First();

            var a = new dynamic[reader.FieldCount];
            if (reader.Read())
            {
                reader.GetValues(a);
                var list = a.ToList();
                _memoryStorage.Add(list);
                yield return list;
            }
            if (!reader.IsClosed)
                reader.Close();
        }

        private IEnumerable<object> GetDataField(object scalar)
        {
            yield return scalar;
        }
    }
}
