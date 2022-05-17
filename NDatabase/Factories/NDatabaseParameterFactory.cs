using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDatabase.Factories
{
    public static class NDatabaseParameterFactory
    {
        public static DbParameter? GetParameter(DbConnection connection, string parameterName, object parameterValue, DbType dbType)
        {
            var providerFactory = DbProviderFactories.GetFactory(connection);
            if (providerFactory is null)
                throw new ArgumentException();

            var parameter = providerFactory.CreateParameter();
            if (parameter is null)
                throw new ArgumentException();

            parameter.ParameterName = parameterName; 
            parameter.Value = parameterValue;
            parameter.DbType = dbType;

            return parameter;
        }
    }
}
