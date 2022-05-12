namespace NDatabase
{
    public partial class NDatabase
    {
        public override int ExecuteNonQuery(
            string sql, IEnumerable<object> parameterCollection,
            bool autoCommit = true)
        {

            if (_connection == null || _transaction == null) return 0;

            var command = GetCommand(sql);
            command.Parameters.AddRange(parameterCollection.ToArray());

            var rowEffected = command.ExecuteNonQuery();

            if (autoCommit && command.Transaction != null)
                command.Transaction.Commit();

            return rowEffected;
        }
        public override List<int> ExecuteManyNonQuery(
            string sql, IEnumerable<IEnumerable<object>> parametersCollection,
            bool autoCommit = true)
        {
            if (_connection == null || _transaction == null) return new List<int>();

            var rowEffected = new List<int>();

            var command = GetCommand(sql);
            command.Prepare();
            foreach (var paramCollection in parametersCollection)
            {
                command.Parameters.Clear();
                command.Parameters.AddRange(paramCollection.ToArray());

                rowEffected.Add(command.ExecuteNonQuery());
            }
            if (autoCommit && command.Transaction != null)
                command.Transaction.Commit();

            return rowEffected;
        }
    }
}
