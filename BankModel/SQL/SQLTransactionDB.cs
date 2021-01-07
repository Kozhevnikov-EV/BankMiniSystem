using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace BankModel_Library
{
    /// <summary>
    /// Статический класс для работы с экземплярами классов Transaction в БД
    /// </summary>
    public static class SQLTransactionDB
    {
        /// <summary>
        /// Возвращает коллекцию всех экземпляров Transaction из БД
        /// </summary>
        /// <returns>ObservableCollection<Transaction></returns>
        public static ObservableCollection<Transaction> GetAllTransactions()
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                            Transactions.Id,
                            Transactions.[name],
                            Transactions.[date],
                            Transactions.fromAccount,
                            Transactions.toAccount,
                            Transactions.[sum],
                            Transactions.[status]
                            FROM Transactions";
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр Transaction из данных БД
                        Transaction transaction = new Transaction(
                            dataReader.GetInt32(0),
                            dataReader.GetString(1),
                            dataReader.GetDateTime(2),
                            dataReader.GetInt32(3),
                            dataReader.GetInt32(4),
                            dataReader.GetDouble(5),
                            dataReader.GetBoolean(6));
                        transactions.Add(transaction);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
            return transactions;
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров Transaction одного счета из БД
        /// </summary>
        /// <param name="AccountId">Номер счета (Id)</param>
        /// <returns>ObservableCollection<Transaction></returns>
        public static ObservableCollection<Transaction> GetTransaction(int AccountId)
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = $@"SELECT
                            Transactions.Id,
                            Transactions.[name],
                            Transactions.[date],
                            Transactions.fromAccount,
                            Transactions.toAccount,
                            Transactions.[sum],
                            Transactions.[status]
                            FROM Transactions
                            WHERE Transactions.Id = {AccountId}";
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр Transaction из данных БД
                        Transaction transaction = new Transaction(
                            dataReader.GetInt32(0),
                            dataReader.GetString(1),
                            dataReader.GetDateTime(2),
                            dataReader.GetInt32(3),
                            dataReader.GetInt32(4),
                            dataReader.GetDouble(5),
                            dataReader.GetBoolean(6));
                        transactions.Add(transaction);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
            return transactions;
        }

        /// <summary>
        /// Добавляет в БД экземпляр Transaction
        /// </summary>
        /// <param name="transaction">Экземпляр Transaction</param>
        public static void AddTransaction(Transaction transaction)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    //формируем запрос T-SQL
                    var sql = @"INSERT INTO Transactions ([name], [date], [fromAccount], [toAccount], [sum], [status])
                            VALUES(@name, @date, @fromAccount, @toAccount, @sum, @status)";
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@name", transaction.Name)); //добавляем параметры в команду
                    command.Parameters.Add(new SqlParameter("@date", transaction.Date));
                    command.Parameters.Add(new SqlParameter("@fromAccount", transaction.FromAccount));
                    command.Parameters.Add(new SqlParameter("@toAccount", transaction.ToAccount));
                    command.Parameters.Add(new SqlParameter("@sum", transaction.Sum));
                    command.Parameters.Add(new SqlParameter("@status", transaction.Status));
                    command.ExecuteNonQuery(); //отправляем команду в БД
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }
    }
}
