using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;


namespace BankModel_Library
{
    /// <summary>
    /// Статический класс для работы со счетами Account, Credit и Deposit в БД
    /// </summary>
    public static class SQLAccountDB
    {
        /// <summary>
        /// Формирует коллекцию всех счетов всех типов из БД
        /// </summary>
        /// <returns>ObservableCollection<Account></returns>
        public static ObservableCollection<Account> GetAccounts()
        {
            ObservableCollection<Account> accounts = new ObservableCollection<Account>();
            GetCheckAccounts(accounts); //наполняем коллекцию текущими счетами
            GetDeposits(accounts); //наполняем коллекцию вкладами
            GetCredits(accounts); //наполняем коллекцию кредитами
            return accounts;
        }

        /// <summary>
        /// Наполняет переданную в метод коллецию счетами базового класса (текущие счета) из БД
        /// </summary>
        /// <param name="accounts">Коллекция, в которую метод добавит экземпляры счетов Account</param>
        private static void GetCheckAccounts(ObservableCollection<Account> accounts)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                            Accounts.Id,
                            Accounts.clientId,
                            Accounts.isOpen,
                            Accounts.isRefill,
                            Accounts.isWithdrawal,
                            Accounts.openDate,
                            Accounts.balance
                            FROM Accounts
                            WHERE Accounts.isCheking = 1"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр Account из данных БД
                        Account account = new Account(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetBoolean(2),
                            dataReader.GetBoolean(3),
                            dataReader.GetBoolean(4),
                            dataReader.GetDateTime(5),
                            dataReader.GetDouble(6));
                        accounts.Add(account);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Наполняет переданную в метод коллецию вкладами из БД
        /// </summary>
        /// <param name="accounts">Коллекция, в которую метод добавит экземпляры Deposit</param>
        public static void GetDeposits(ObservableCollection<Account> accounts)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                            Accounts.Id,
                            Accounts.clientId,
                            Accounts.isOpen,
                            Accounts.isRefill,
                            Accounts.isWithdrawal,
                            Accounts.openDate,
                            Accounts.balance,
                            Deposits.[percent],
                            Deposits.capitalization,
                            Deposits.endDate,
                            Deposits.previousCapitalization,
                            Deposits.isActive
                            FROM Accounts, Deposits
                            WHERE Accounts.isCheking = 0 AND Accounts.Id = Deposits.Id"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read()) //считываем полученные данные от БД
                    {
                        //формируем экзепляр Deposit из данных БД
                        Deposit deposit = new Deposit(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetBoolean(2),
                            dataReader.GetBoolean(3),
                            dataReader.GetBoolean(4),
                            dataReader.GetDateTime(5),
                            dataReader.GetDouble(6),
                            dataReader.GetDouble(7),
                            dataReader.GetBoolean(8),
                            dataReader.GetDateTime(9),
                            dataReader.GetDateTime(10),
                            dataReader.GetBoolean(11));
                        accounts.Add(deposit);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Наполняет переданную в метод коллецию кредитами из БД
        /// </summary>
        /// <param name="accounts">Коллекция, в которую метод добавит экземпляры Credit</param>
        public static void GetCredits(ObservableCollection<Account> accounts)
        {
            using (SqlConnection con = new SqlConnection(SQLConString.Str()))
            {
                try
                {
                    con.Open(); //открываем соединение
                    var sql = @"SELECT
                            Accounts.Id,
                            Accounts.clientId,
                            Accounts.isOpen,
                            Accounts.isRefill,
                            Accounts.isWithdrawal,
                            Accounts.openDate,
                            Accounts.balance,
                            Credits.[percent],
                            Credits.endDate,
                            Credits.previousCapitalization,
                            Credits.isActive,
                            Credits.startDebt,
                            Credits.curentDebt,
                            Credits.monthlyPayment,
                            Credits.withoutNegativeBalance
                            FROM Accounts, Credits
                            WHERE Accounts.isCheking = 0 AND Accounts.Id = Credits.Id"; //формируем T-SQL запрос
                    SqlCommand command = new SqlCommand(sql, con); //создаем экземпляр команды
                    SqlDataReader dataReader = command.ExecuteReader(); //отправляем команду в БД
                    while (dataReader.Read())  //считываем полученные данные от БД
                    {
                        //формируем экзепляр Credit из данных БД
                        Credit credit = new Credit(
                            dataReader.GetInt32(0),
                            dataReader.GetInt32(1),
                            dataReader.GetBoolean(2),
                            dataReader.GetBoolean(3),
                            dataReader.GetBoolean(4),
                            dataReader.GetDateTime(5),
                            dataReader.GetDouble(6),
                            dataReader.GetDouble(7),
                            dataReader.GetDateTime(8),
                            dataReader.GetDateTime(9),
                            dataReader.GetBoolean(10),
                            dataReader.GetDouble(11),
                            dataReader.GetDouble(12),
                            dataReader.GetDouble(13),
                            dataReader.GetBoolean(14));
                        accounts.Add(credit);
                    }
                }
                catch (Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }

        /// <summary>
        /// Метод добавления в БД экземпляра Account
        /// </summary>
        /// <param name="e">Экзепляр Account</param>
        public static void AddCheckAccount(Account e)
        {
            string sql = $@"INSERT INTO Accounts
                            (clientId, openDate)
                            VALUES (@clientId, @openDate)"; //запрос в T-SQL
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@clientId", e.ClientId)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@openDate", e.OpenDate));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Метод добавления в БД экземпляра Deposit
        /// </summary>
        /// <param name="e">Экзепляр Deposit</param>
        public static void AddDeposit(Deposit e)
        {
            //запрос в T-SQL
            string sql = $@"INSERT INTO Accounts
                            (clientId, isOpen, isRefill, isWithdrawal, openDate, balance, isCheking)
                            VALUES (@clientId, @isOpen, @isRefill, @isWithdrawal, @openDate, @balance, @isCheking)
                            INSERT INTO Deposits
                            (Id, [percent], capitalization, endDate, previousCapitalization, isActive)
                            VALUES (@@IDENTITY, @percent, @capitalization, @endDate, @previousCapitalization, @isActive)";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@clientId", e.ClientId)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@isOpen", e.IsOpen));
            command.Parameters.Add(new SqlParameter("@isRefill", e.IsRefill));
            command.Parameters.Add(new SqlParameter("@isWithdrawal", e.IsWithdrawal));
            command.Parameters.Add(new SqlParameter("@openDate", e.OpenDate));
            command.Parameters.Add(new SqlParameter("@balance", e.Balance));
            command.Parameters.AddWithValue("@isCheking", false);
            command.Parameters.Add(new SqlParameter("@percent", e.Percent));
            command.Parameters.Add(new SqlParameter("@capitalization", e.Capitalization));
            command.Parameters.Add(new SqlParameter("@endDate", e.EndDate));
            command.Parameters.Add(new SqlParameter("@previousCapitalization", e.PreviousCapitalization));
            command.Parameters.Add(new SqlParameter("@isActive", e.IsActive));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Метод добавления в БД экземпляра Credit
        /// </summary>
        /// <param name="e">Экзепляр Credit</param>
        public static void AddCredit(Credit e)
        {
            //запрос в T-SQL
            string sql = $@"INSERT INTO Accounts
                            (clientId, isOpen, isRefill, isWithdrawal, openDate, balance, isCheking)
                            VALUES (@clientId, @isOpen, @isRefill, @isWithdrawal, @openDate, @balance, @isCheking)
                   INSERT INTO Credits
                   (Id, [percent], startDebt, curentDebt, monthlyPayment, endDate, previousCapitalization, isActive, withoutNegativeBalance)
                   VALUES (@@IDENTITY, @percent, @startDebt, @curentDebt, @monthlyPayment, @endDate, @previousCapitalization, @isActive, @withoutNegativeBalance)";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@clientId", e.ClientId)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@isOpen", e.IsOpen));
            command.Parameters.Add(new SqlParameter("@isRefill", e.IsRefill));
            command.Parameters.Add(new SqlParameter("@isWithdrawal", e.IsWithdrawal));
            command.Parameters.Add(new SqlParameter("@openDate", e.OpenDate));
            command.Parameters.Add(new SqlParameter("@balance", e.Balance));
            command.Parameters.AddWithValue("@isCheking", false);
            command.Parameters.Add(new SqlParameter("@percent", e.Percent));
            command.Parameters.Add(new SqlParameter("@startDebt", e.StartDebt));
            command.Parameters.Add(new SqlParameter("@curentDebt", e.CurentDebt));
            command.Parameters.Add(new SqlParameter("@monthlyPayment", e.MonthlyPayment));
            command.Parameters.Add(new SqlParameter("@endDate", e.EndDate));
            command.Parameters.Add(new SqlParameter("@previousCapitalization", e.PreviousCapitalization));
            command.Parameters.Add(new SqlParameter("@isActive", e.IsActive));
            command.Parameters.Add(new SqlParameter("@withoutNegativeBalance", e.WithoutNegativeBalance));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Метод, редактирующий в БД переданный экземпляр Account, Credit или Deposit
        /// </summary>
        /// <typeparam name="T">Тип передаваемого методу класса, унаследованного от Account</typeparam>
        /// <param name="account">Экземпляр Account, Deposit или Credit</param>
        public static void UpdateAccount<T>(T account)
            where T : Account
        {
            //в зависимости от переданного типа account используем соотвествующий метод
            if (account.GetType() == typeof(Deposit)) UpdateDeposit(account as Deposit);
            else if (account.GetType() == typeof(Credit)) UpdateCredit(account as Credit);
            else if (account.GetType() == typeof(Account)) UpdateCheckAccount(account as Account);
        }

        /// <summary>
        /// Метод, редактирующий в БД переданный экземпляр текущего счета Account
        /// </summary>
        /// <param name="e">Экзепляр Account</param>
        private static void UpdateCheckAccount(Account e)
        {
            //запрос в T-SQL
            string sql = $@"UPDATE Accounts SET
                            [isOpen] = @isOpen,
                            [isRefill] = @isRefill,
                            [isWithdrawal] = @isWithdrawal,
                            [balance] = @balance
                            WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@isOpen", e.IsOpen)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@isRefill", e.IsRefill));
            command.Parameters.Add(new SqlParameter("@isWithdrawal", e.IsWithdrawal));
            command.Parameters.Add(new SqlParameter("@balance", e.Balance));
            command.Parameters.Add(new SqlParameter("@Id", e.Id));
            TransactCommand(command); //проводим команду в БД

        }

        /// <summary>
        /// Метод, редактирующий в БД переданный экземпляр вклада Deposit
        /// </summary>
        /// <param name="e">Экзепляр Deposit</param>
        private static void UpdateDeposit(Deposit e)
        {
            //запрос в T-SQL
            string sql = $@"UPDATE Accounts SET
                            [isOpen] = @isOpen,
                            [isRefill] = @isRefill,
                            [isWithdrawal] = @isWithdrawal,
                            [balance] = @balance
                            WHERE Id = @Id
                            UPDATE Deposits SET
                            [previousCapitalization] = @previousCapitalization,
                            [isActive] = @isActive                            
                            WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@isOpen", e.IsOpen)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@isRefill", e.IsRefill));
            command.Parameters.Add(new SqlParameter("@isWithdrawal", e.IsWithdrawal));
            command.Parameters.Add(new SqlParameter("@balance", e.Balance));
            command.Parameters.Add(new SqlParameter("@previousCapitalization", e.PreviousCapitalization));
            command.Parameters.Add(new SqlParameter("@isActive", e.IsActive));
            command.Parameters.Add(new SqlParameter("@Id", e.Id));
            TransactCommand(command); //проводим команду в БД
        }

        /// <summary>
        /// Метод, редактирующий в БД переданный экземпляр вклада Credit
        /// </summary>
        /// <param name="e">Экзепляр Credit</param>
        private static void UpdateCredit(Credit e)
        {
            //запрос в T-SQL
            string sql = $@"UPDATE Accounts SET
                            [isOpen] = @isOpen,
                            [isRefill] = @isRefill,
                            [isWithdrawal] = @isWithdrawal,
                            [balance] = @balance
                            WHERE Id = @Id
                            UPDATE Credits SET
                            [curentDebt] = @curentDebt,
                            [previousCapitalization] = @previousCapitalization,
                            [isActive] = @isActive,
                            [withoutNegativeBalance] = @withoutNegativeBalance
                            WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql); //создаем экземпляр команды
            command.Parameters.Add(new SqlParameter("@isOpen", e.IsOpen)); //добавляем в нее параметры
            command.Parameters.Add(new SqlParameter("@isRefill", e.IsRefill));
            command.Parameters.Add(new SqlParameter("@isWithdrawal", e.IsWithdrawal));
            command.Parameters.Add(new SqlParameter("@balance", e.Balance));
            command.Parameters.Add(new SqlParameter("@curentDebt", e.CurentDebt));
            command.Parameters.Add(new SqlParameter("@previousCapitalization", e.PreviousCapitalization));
            command.Parameters.Add(new SqlParameter("@isActive", e.IsActive));
            command.Parameters.Add(new SqlParameter("@withoutNegativeBalance", e.WithoutNegativeBalance));
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
                catch(Exception Ex)
                {
                    Bank.InvokeExeptionEvent(Ex);
                }
            }
        }
    }
}
