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
    /// Статический класс для работы с экземплярами классов ActivityInfo в БД
    /// </summary>
    public static class SQLActivityInfoDB
    {
        /// <summary>
        /// Метод, возвращающий коллекцию с экземплярами ActivityInfo
        /// </summary>
        /// <returns>ObservableCollection<ActivityInfo></returns>
        public static ObservableCollection<ActivityInfo> GetLog()
        {
            ObservableCollection<ActivityInfo> infos = new ObservableCollection<ActivityInfo>();
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    var sql = @"SELECT
                            ActivityInfo.[date],
                            ActivityInfo.[message]
                            FROM ActivityInfo
                            ORDER BY [date] DESC"; //формируем запрос T-SQL
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр ActivityInfo из данных БД
                        ActivityInfo info = new ActivityInfo(dataReader.GetDateTime(0), dataReader.GetString(1));
                        infos.Add(info);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
            return infos;
        }

        /// <summary>
        /// Добавляет в БД экземпляр ActivityInfo
        /// </summary>
        /// <param name="info">Экземпляр ActivityInfo</param>
        public static void AddLogToDB(ActivityInfo info)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    var sql = @"INSERT INTO ActivityInfo ([date], [message])
                            VALUES (@date, @message)";  //формируем запрос T-SQL
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@date", info.Date)); //добавляем параметры в команду
                    command.Parameters.Add(new SqlParameter("@message", info.Message));
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
