using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace BankModel_Library
{
    /// <summary>
    /// Статический класс, содержащий объект - ConnectionStringBuilder с актуальной строкой подключения
    /// </summary>
    public static class SQLConString
    {
        private static SqlConnectionStringBuilder _sqlСonnect;
        /// <summary>
        /// Строка подключения
        /// </summary>
        public static SqlConnectionStringBuilder sqlConnect
        { 
            get
            {
                if (_sqlСonnect == null)
                {
                    SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder()
                    {
                        DataSource = @"(localdb)\MSSQLLocalDB",
                        InitialCatalog = "MSSQLLocalBankDB",
                        IntegratedSecurity = true,
                        Pooling = true
                    };
                    return sql;
                }
                else return _sqlСonnect;
            }
            set { _sqlСonnect = value; }
        }

        /// <summary>
        /// Метод, изменяющий sqlConnect
        /// </summary>
        public static void EditConnection(string dataSource, string initialCatalog, bool integtatedSecurity, bool pooling)
        {
            sqlConnect = new SqlConnectionStringBuilder()
            {
                DataSource = dataSource,
                InitialCatalog = initialCatalog,
                IntegratedSecurity = integtatedSecurity,
                Pooling = pooling
            };
        }

        public static string Str()
        {
            return sqlConnect.ConnectionString;
        }


    }
}
