using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace BankModel_Library
{
    /// <summary>
    /// Статический класс для работы с экземплярами классов BalanceLog в БД
    /// </summary>
    public static class SQLBalanceLogDB
    {
        /// <summary>
        /// Метод, возвращающий коллекцию экземпляров BalanceLog из БД по номеру AccountId
        /// </summary>
        /// <param name="AccountId">Номер счета</param>
        /// <returns>List<BalanceLog></returns>
        public static List<BalanceLog> GetLogCol(int AccountId)
        {
            List<BalanceLog> logs = new List<BalanceLog>();
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    var sql = $@"SELECT
                            BalanceLog.[date],
                            BalanceLog.[accountId],
                            BalanceLog.[message],
                            BalanceLog.balance,
                            BalanceLog.transactionId
                            FROM BalanceLog
                            WHERE BalanceLog.accountId = @accountId"; //формируем запрос T-SQL
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@accountId", AccountId)); //добавляем параметр в команду
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр BalanceLog из данных БД
                        BalanceLog log = new BalanceLog(
                            dataReader.GetDateTime(0),
                            dataReader.GetInt32(1),
                            dataReader.GetString(2),
                            dataReader.GetDouble(3),
                            dataReader.GetInt32(4));
                        logs.Add(log);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
            return logs;
        }

        /// <summary>
        /// Добавляет в БД экземпляр BalanceLog
        /// </summary>
        /// <param name="log">Экзепляр BalanceLog</param>
        public static void AddBalanceLogToDB(BalanceLog log)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    //формируем запрос T-SQL
                    var sql = @"INSERT INTO BalanceLog (accountId, [date], [message], [balance], transactionId)
                            VALUES (@accountId, @date, @message, @balance, @transactionId)";
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@accountId", log.AccountId));  //добавляем параметры в команду
                    command.Parameters.Add(new SqlParameter("@date", log.Date));
                    command.Parameters.Add(new SqlParameter("@message", log.Message));
                    command.Parameters.Add(new SqlParameter("@balance", log.Balance));
                    command.Parameters.Add(new SqlParameter("@transactionId", log.TransactionId));
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
