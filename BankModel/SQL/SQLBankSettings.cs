using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using InvalidCharException_Library;

namespace BankModel_Library
{
    /// <summary>
    /// Статический класс для работы с свойствами класса Bank в БД
    /// </summary>
    public static class SQLBankSettings
    {
        /// <summary>
        /// Метод, устанавливающий свойства экземпляра Bank и статические свойства класса Bank из данных БД
        /// </summary>
        /// <param name="bank">Экземпляр Bank</param>
        public static void SetBankFields(Bank bank)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    //делаем предварительный запрос, есть ли в БД информация
                    var sql = @"SELECT COUNT(*) FROM Bank";
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    int count = (int)command.ExecuteScalar(); //отправляем команду в БД
                    if (count != 0) //если таблица Bank не пустая
                    {
                        sql = @"SELECT
                            [today],
                            [name],
                            [baseRate],
                            [illegalChars]
                            FROM Bank";  //формируем запрос T-SQL
                        command = new SqlCommand(sql, connection); //создаем экземпляр команды
                        SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                        dataReader.Read(); //считываем полученные данные от БД
                        Bank.Today = dataReader.GetDateTime(0); //присваиваем текущую дату банка
                        bank.Name = dataReader.GetString(1); //присваиваем имя экземпляру банка bank
                        Bank.BaseRate = dataReader.GetDouble(2); //присваиваем базовую процентную ставку
                        if (!dataReader.IsDBNull(3)) InvalidCharException.CharCollection = dataReader.GetString(3).ToArray();
                        //устаналиваем недопустимые символы в наименовании банка, если таковые имеются в БД
                    }
                    else //если таблица Bank пуста
                    {
                        Bank.Today = DateTime.Now;
                        Bank.BaseRate = 10;
                        bank.Name = "Мой банк";
                        InvalidCharException.CharCollection = String.Empty.ToArray();
                        AddSettingsToDB(bank);
                    }

                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Обновляет свойства Bank в БД
        /// </summary>
        /// <param name="bank">Экземпляр Bank</param>
        public static void UpdateBankSettingsInDB(Bank bank)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    var sql = @"UPDATE Bank SET
                            [today] = @today,
                            [name] = @name,
                            [baseRate] = @baseRate,
                            [illegalChars] = @illegalChars
                            WHERE Bank.Id = 1"; //формируем запрос T-SQL
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@today", Bank.Today)); //добавляем параметры команды
                    command.Parameters.Add(new SqlParameter("@name", bank.Name));
                    command.Parameters.Add(new SqlParameter("@baseRate", Bank.BaseRate));
                    command.Parameters.Add(new SqlParameter("@illegalChars", InvalidCharException.CharCollection));
                    command.ExecuteNonQuery(); //отправляем команду в БД
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Создает первую и единственную запись в БД (таблица Bank)
        /// </summary>
        /// <param name="bank"></param>
        private static void AddSettingsToDB(Bank bank)
        {
            using (SqlConnection connection = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    connection.Open(); //открываем соединение
                    var sql = @"INSERT INTO Bank ([today], [name], [baseRate], [illegalChars])
                            VALUES (@today, @name, @baseRate, @illegalChars)"; //формируем запрос T-SQL
                    SqlCommand command = new SqlCommand(sql, connection); //создаем экземпляр команды
                    command.Parameters.Add(new SqlParameter("@today", Bank.Today)); //добавляем параметры команды
                    command.Parameters.Add(new SqlParameter("@name", bank.Name));
                    command.Parameters.Add(new SqlParameter("@baseRate", Bank.BaseRate));
                    command.Parameters.Add(new SqlParameter("@illegalChars", InvalidCharException.CharCollection));
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
