using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace BankModel_Library
{
    /// <summary>
    /// Статический класс для работы со клиентами NaturalPerson, VIP и Company в БД
    /// </summary>
    public static class SQLClientDB
    {
        /// <summary>
        /// Формирует коллекцию всех клиентов из БД
        /// </summary>
        /// <returns>ObservableCollection<Client></returns>
        public static ObservableCollection<Client> GetClients()
        {
            ObservableCollection<Client> clients = new ObservableCollection<Client>();
            GetNaturals(clients); //наполняем коллекцию физ.лицами
            GetVIPs(clients); //наполняем коллекцию VIP клиентами
            GetCompanies(clients); //наполняем коллекцию юр.лицами
            return clients;
        }

        /// <summary>
        /// Наполняет переданную коллекцию клиентами - физ.лицами NaturalPerson из БД
        /// </summary>
        /// <param name="clients">Коллекция клиентов</param>
        private static void GetNaturals(ObservableCollection<Client> clients)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                        Clients.Id,
                        Clients.creditRating,
                        NaturalPersons.[name],
                        NaturalPersons.surname,
                        NaturalPersons.birthday
                        FROM Clients, NaturalPersons
                        WHERE Clients.Id = NaturalPersons.Id"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр NaturalPerson из данных БД
                        NaturalPerson natural = new NaturalPerson(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetString(2),
                            dataReader.GetString(3),
                            dataReader.GetDateTime(4));
                        clients.Add(natural);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Наполняет переданную коллекцию вип-клиентами VIP из БД
        /// </summary>
        /// <param name="clients">Коллекция клиентов</param>
        private static void GetVIPs(ObservableCollection<Client> clients)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                    Clients.Id,
                    Clients.creditRating,
                    VIPs.[name],
                    VIPs.surname,
                    VIPs.birthday,
                    VIPs.workPlace
                    FROM Clients, VIPs
                    WHERE Clients.Id = VIPs.Id"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр VIP из данных БД
                        VIP vip = new VIP(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetString(2),
                            dataReader.GetString(3),
                            dataReader.GetDateTime(4),
                            dataReader.GetString(5));
                        clients.Add(vip);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Наполняет переданную коллекцию клиентами-юр.лицами Company из БД
        /// </summary>
        /// <param name="clients">Коллекция клиентов</param>
        private static void GetCompanies(ObservableCollection<Client> clients)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                    Clients.Id,
                    Clients.creditRating,
                    Companies.typeOrg,
                    Companies.[name]
                    FROM Clients, Companies
                    WHERE Clients.Id = Companies.Id"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр Company из данных БД
                        Company vip = new Company(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetString(2),
                            dataReader.GetString(3));
                        clients.Add(vip);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Добавляет экземпляр NaturalPerson в БД
        /// </summary>
        /// <param name="e">Экземпляр NaturalPerson</param>
        public static void AddNatural(NaturalPerson e)
        {
            //формируем T-SQL запрос
            string sql = $@"INSERT INTO Clients (creditRating) 
                                VALUES (@creditRating)
                    INSERT INTO NaturalPersons (Id, [name], surname, birthday)
                                VALUES (@@IDENTITY, @name, @surname, @birthday)";
            SqlCommand command = new SqlCommand(sql);  //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@creditRating", e.CreditRating)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@name", e.Name));
            command.Parameters.Add(new SqlParameter("@surname", e.Surname));
            command.Parameters.Add(new SqlParameter("@birthday", e.Birthday));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Добавляет экземпляр VIP в БД
        /// </summary>
        /// <param name="e">Экземпляр VIP</param>
        public static void AddVIP(VIP e)
        {
            //формируем T-SQL запрос
            string sql = $@"INSERT INTO Clients (creditRating) 
                                VALUES (@creditRating)
                    INSERT INTO VIPs (Id, [name], surname, birthday, workPlace)
                                VALUES (@@IDENTITY, @name, @surname, @birthday, @workPlace)";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@creditRating", e.CreditRating)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@name", e.Name));
            command.Parameters.Add(new SqlParameter("@surname", e.Surname));
            command.Parameters.Add(new SqlParameter("@birthday", e.Birthday));
            command.Parameters.Add(new SqlParameter("@workPlace", e.WorkPlace));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Добавляет экземпляр Company в БД
        /// </summary>
        /// <param name="e">Экземпляр Company</param>
        public static void AddCompany(Company e)
        {
            //формируем T-SQL запрос
            string sql = $@"INSERT INTO Clients (creditRating) 
                                VALUES (@creditRating)
                    INSERT INTO Companies (Id, typeOrg, [name])
                                VALUES (@@IDENTITY, @typeOrg, @name)";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@creditRating", e.CreditRating)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@typeOrg", e.TypeOrg));
            command.Parameters.Add(new SqlParameter("@name", e.Name));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Редактирует переданный экземпляр NaturalPerson в БД
        /// </summary>
        /// <param name="e">Экземпляр NaturalPerson</param>
        public static void EditNatural(NaturalPerson e)
        {
            //формируем T-SQL запрос
            string sql = $@"UPDATE NaturalPersons SET
                           [name] = @name,
                           surname = @surname,
                           birthday = @birthday
                           WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@name", e.Name)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@surname", e.Surname));
            command.Parameters.Add(new SqlParameter("@birthday", e.Birthday));
            command.Parameters.Add(new SqlParameter("@Id", e.Id));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Редактирует переданный экземпляр VIP в БД
        /// </summary>
        /// <param name="e">Экземпляр VIP</param>
        public static void EditVIP(VIP e)
        {
            //формируем T-SQL запрос
            string sql = $@"UPDATE VIPs SET
                           [name] = @name,
                           surname = @surname,
                           birthday = @birthday,
                           workPlace = @workPlace
                           WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@name", e.Name)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@surname", e.Surname));
            command.Parameters.Add(new SqlParameter("@birthday", e.Birthday));
            command.Parameters.Add(new SqlParameter("@workPlace", e.WorkPlace));
            command.Parameters.Add(new SqlParameter("@Id", e.Id));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Редактирует переданный экземпляр Company в БД
        /// </summary>
        /// <param name="e">Экземпляр Company</param>
        public static void EditCompany(Company e)
        {
            //формируем T-SQL запрос
            string sql = $@"UPDATE Companies SET
                           [typeOrg] = @typeOrg,
                           [name] = @name
                           WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@typeOrg", e.TypeOrg)); //добавляем параметры команды
            command.Parameters.Add(new SqlParameter("@name", e.Name));
            command.Parameters.Add(new SqlParameter("@Id", e.Id));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Метод, который проводит переданную команду в БД
        /// </summary>
        /// <param name="command">Команда</param>
        private static void TransactCommand(SqlCommand command)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение с БД
                    command.Connection = con; //добавляем в команду текущее соединение
                    command.ExecuteNonQuery(); //проводим команду в БД
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }
    }
}
