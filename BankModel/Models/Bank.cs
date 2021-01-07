using Newtonsoft.Json;
using BankExtensions_Library;
using InvalidCharException_Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace BankModel_Library
{
    public class Bank
    {
        #region Поля и свойства

        /// <summary>
        /// Событие "Исключение" возникает при исключении в экземпляре Bank
        /// </summary>
        public static event Action<Exception> BankException;

        /// <summary>
        /// Отображает текущую дату
        /// </summary>
        public static DateTime Today { get; set; }

        /// <summary>
        /// Базовая ставка - поле
        /// </summary>
        private static double baseRate;
        
        /// <summary>
        /// Базовая ставка % (применяется для расчета % по кредитам и вкладам)
        /// </summary>
        public static double BaseRate
        {
            get { return baseRate; }
            set { baseRate = (value > 0 && value <= 100) ? value : 10; }
        }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Коллекция экземпляров клиентов
        /// </summary>
        public ObservableCollection<Client> Clients { get; private set; }

        /// <summary>
        /// Коллекция экземпляров счетов
        /// </summary>
        public ObservableCollection<Account> Accounts { get; private set; }

        /// <summary>
        /// Коллекция всех операций по всем счетам
        /// </summary>
        public ObservableCollection<Transaction> Transactions { get; private set; }

        /// <summary>
        /// Коллекция с логами
        /// </summary>
        public ObservableCollection<ActivityInfo> LogList { get; set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Базовый конструктор, создает экземпляр банка из данных БД
        /// </summary>
        public Bank()
        {
            SQLBankSettings.SetBankFields(this);
            Clients = SQLClientDB.GetClients();
            Accounts = SQLAccountDB.GetAccounts();
            Transactions = SQLTransactionDB.GetAllTransactions();
            LogList = SQLActivityInfoDB.GetLog();
            SubscribeBankItemsToEvenst();
        }
        #endregion

        public static void InvokeExeptionEvent(Exception Ex)
        {
            BankException?.Invoke(Ex);
        }

        #region Методы работы, подписывающие свойства банка на события!

        /// <summary>
        /// Подписывает клиентов на событие "Закрытие кредита" (CreditFinished) их кредитных счетов (Credit) и подписывает счета
        /// на событие "Изменение в балансе"
        /// </summary>
        /// <param name="bank">Экземпляр Bank, клиентов и счета которого надо подписывать на события</param>
        private void SubscribeBankItemsToEvenst()
        {
            foreach (var account in Accounts) //перебираем в цикле все счета
            {
                account.Activity += AddActivityToLogListAndBalanceLog; //и все счета подписываем на событие "Изменение баланса"
                if (account is Credit) //если натыкаемся на кредит
                {
                    var clients = Clients.Where(e => e.Id == account.ClientId); //то находим клиента - владельца кредита
                    foreach (var client in clients)
                    {
                        (account as Credit).CreditFinished += client.ChangeCreditRating; //и подписываем его на событие закрытия кредита
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет коллекцию счетов банка из БД и подписывает их на события
        /// </summary>
        private void UpdateAccountsColFromDB()
        {
            Accounts = SQLAccountDB.GetAccounts();
            SubscribeBankItemsToEvenst();
        }

        /// <summary>
        /// Обновляет коллекцию клиентов банка из БД
        /// </summary>
        private void UpdateClientsColFromDB()
        {
            Clients = SQLClientDB.GetClients();
        }
        #endregion

        #region Методы присвоения значений полям банка (Name, BaseRate) + ограничение на использование символов

        /// <summary>
        /// Устанавливает наименование банка, базовую процентную ставку и неиспользуемые в наименовании символы
        /// </summary>
        /// <param name="Name">Наименование банка</param>
        /// <param name="BaseRate">Базовая процентная ставка</param>
        /// <param name="IllegalChars">Неиспользуемые (запрещенные) символы</param>
        /// <returns>string - фактически присвоенные значения</returns>
        public string BankSettings(string Name, double BaseRate, params char[] IllegalChars)
        {
            try
            {
                InvalidCharException.CharCollection = IllegalChars; //устанавливаем в свойство исключения набор недопустимых символов
                                                                    //проверяем введенное имя на наличие недопустимых символов
                if (Name.ContainsChars(InvalidCharException.CharCollection)) throw new InvalidCharException(); 
                //если находим - выбрасываем исключение
            }
            catch (Exception Ex)
            {
                BankException?.Invoke(Ex);
            }
            finally
            {
                Name = Name.TrimAllChars(InvalidCharException.CharCollection); //удаляем недопустимые символы из наименования
                this.Name = Name; //и присваиваем банку новое имя
            }
            Bank.BaseRate = BaseRate;
            SQLBankSettings.UpdateBankSettingsInDB(this); //обновляем данные в БД
            return $"Наименоваение банка: {this.Name}. \nБазовая ставка: {Bank.BaseRate} %";
        }
        #endregion

        #region Методы добавления/изменения клиентов
        /// <summary>
        /// Создает экземпляр физ. лица
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        public void AddNatural(string Name, string Surname, DateTime Birthday)
        {
            NaturalPerson Natural = new NaturalPerson(Name, Surname, Birthday); //создаем экземпляр физ лица
            SQLClientDB.AddNatural(Natural); //добавляем его в БД
            UpdateClientsColFromDB(); //обновляем коллекцию клиентов из БД
        }

        /// <summary>
        /// Создает экземпляр VIP клиента
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        /// <param name="WorkPlace">Место работы</param>
        public void AddVIP(string Name, string Surname, DateTime Birthday, string WorkPlace)
        {
            VIP VIP = new VIP(Name, Surname, Birthday, WorkPlace); //создаем экземпляр ВИП клиента
            SQLClientDB.AddVIP(VIP); //добавляем его в БД
            UpdateClientsColFromDB(); //обновляем коллекцию клиентов из БД
        }

        /// <summary>
        /// Создает экземпляр юр. лица
        /// </summary>
        /// <param name="TypeOrg">Тип организации</param>
        /// <param name="Name">Наименование</param>
        public void AddCompany(string TypeOrg, string Name)
        {
            Company Company = new Company(TypeOrg, Name); //создаем экземпляр юр. лица
            SQLClientDB.AddCompany(Company); //добавляем его в БД
            UpdateClientsColFromDB(); //обновляем коллекцию клиентов из БД
        }

        /// <summary>
        /// Изменяет поля и свойства экземпляров классов VIP, NaturalPerson и Company
        /// </summary>
        /// <typeparam name="T">Класс, унаследованный от Client</typeparam>
        /// <param name="client">Экземпляр класса, унаследованного от Client</param>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        /// <param name="WorkPlace">Место работы</param>
        /// <param name="TypeOrg">Тип организации</param>
        /// <param name="NameOrg">Наименование организации</param>
        public void EditClient<T>(T client, string Name, string Surname, DateTime Birthday, string WorkPlace, string TypeOrg, string NameOrg)
            where T : Client
        {
            if (client.GetType() == typeof(NaturalPerson)) //если клиент - физ.лицо
            { 
                (client as NaturalPerson).Edit(Name, Surname, Birthday); //правим клиента
                SQLClientDB.EditNatural(client as NaturalPerson); //обновляем его в БД
            }
            else if (client.GetType() == typeof(VIP)) //если клиент - VIP
            { 
                (client as VIP).Edit(Name, Surname, Birthday, WorkPlace); //правим клиента
                SQLClientDB.EditVIP(client as VIP); //обновляем его в БД
            }
            else if (client.GetType() == typeof(Company)) //если клиент - юр.лицо
            { 
                (client as Company).Edit(TypeOrg, NameOrg); //правим клиента
                SQLClientDB.EditCompany(client as Company); //обновляем его в БД
            }
            UpdateClientsColFromDB(); //обновляем коллекцию клиентов банка из БД
        }
        #endregion

        #region Методы работы со счетами + подписка счетов на события
        /// <summary>
        /// Создает счет
        /// </summary>
        /// <param name="ClientId">Id клиента</param>
        public void AddAccount(int ClientId)
        {
            Account account = new Account(ClientId, 0); //добавляем счет, привязывая его к клиентскому Id
            SQLAccountDB.AddCheckAccount(account); //добавляем счет в БД
            UpdateAccountsColFromDB(); //обновляем коллекцию счетов банка из БД
        }

        /// <summary>
        /// Закрывает счет
        /// </summary>
        /// <param name="AccountId">Id счета</param>
        public void CloseAccount(int AccountId)
        {
            foreach (var e in Accounts) //ищем счет в коллекции и закрываем его
            {
                if (e.Id == AccountId && e.IsOpen == true) 
                { 
                    e.IsOpen = false;
                    SQLAccountDB.UpdateAccount(e); //обновляем счет в БД
                    break; 
                }
            }
            UpdateAccountsColFromDB(); //обновляем коллекцию счетов банка из БД
        }

        /// <summary>
        /// Добавляет Вклад указанному экземпляру Client
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="Sum">Сумма вклада</param>
        /// <param name="Percent">Проценты по вкладу</param>
        /// <param name="Capitalization">Капитализация по вкладу</param>
        /// <param name="duration">Срок действия вклада</param>
        public void AddDeposit(Client client, double Sum, double Percent, bool Capitalization, int duration)
        {
            DateTime EndDate = Today.AddMonths(duration); //рассчитываем дату окончания вклада
            Deposit deposit = new Deposit(client.Id, Sum, Percent, Capitalization, EndDate); //создаем экземпляр вклада
            SQLAccountDB.AddDeposit(deposit); //добавляем его в БД
            UpdateAccountsColFromDB();  //обновляем коллекцию счетов банка из БД
        }

        /// <summary>
        /// Добавляет Кредит указанному экземпляру Client
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="StartBalance">Стартовая сумма, внесенная клиентом на счет из личных средств</param>
        /// <param name="Percent">Процент по кредиту</param>
        /// <param name="CreditSum">Сумма кредита (зачисляется на кредитный счет и может быть снята клиентом)</param>
        /// <param name="duration">Срок кредита</param>
        public void AddCredit(Client client, double StartBalance, double Percent, double CreditSum,int duration)
        {
            DateTime EndDate = Today.AddMonths(duration); //Рассчитываем дату окончания кредита
            Credit credit = new Credit(client.Id, StartBalance, Percent, EndDate, CreditSum); //создаем экземпляр кредита
            SQLAccountDB.AddCredit(credit); //добавляем его в БД
            UpdateAccountsColFromDB(); //обновляем коллекцию счетов банка из БД
        }

        /// <summary>
        /// Объединяет два счета в один с наибольшим балансом. Закрывает счет с нулевым балансом.
        /// </summary>
        /// <param name="ClientId"></param>
        public string ConsolidationAccounts(Account account1, Account account2)
        {
            if (account1.GetType() == typeof(Account) && account2.GetType() == typeof(Account))
            {
                if (account1 > account2)
                {
                    CreateTransaction(Transaction.TypeTransaction.CashlessTransfer, account2, account1, account2.Balance);
                    CloseAccount(account2.Id);
                }
                else
                {
                    CreateTransaction(Transaction.TypeTransaction.CashlessTransfer, account1, account2, account1.Balance);
                    CloseAccount(account1.Id);
                }
                return $"Счета объединены";
            }
            else return "Объединяемы счета не должны быть кредитами или вкладами";
        }

        /// <summary>
        /// Рассчитывает процент по Вкладу исходя из текущей базовой ставки и кредитного рейтинга клиента
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <returns>Процент в double</returns>
        public double GetActualDepositPercent(Client client)
        {
            double Percent = Bank.BaseRate + client.CreditRating;
            Percent = (Percent < 0.01) ? 0.01 : Percent;
            Percent = (Percent > 100) ? 100 : Percent;
            return Percent;
        }

        /// <summary>
        /// Рассчитывает процент по Кредиту исходя из текущей базовой ставки и кредитного рейтинга клиента
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <returns>Процент в double</returns>
        public double GetActualCreditPercent(Client client)
        {
            double Percent = Bank.BaseRate - client.CreditRating;
            Percent = (Percent < 0.01) ? 0.01 : Percent;
            Percent = (Percent > 100) ? 100 : Percent;
            return Percent;
        }
        #endregion

        #region Методы создания транзакций по счетам
        /// <summary>
        /// Создает экземпляр транзакции
        /// </summary>
        /// <param name="typeTransaction">Enum транзакции</param>
        /// <param name="FromAccount">Счет отправителя</param>
        /// <param name="ToAccount">Счет получателя</param>
        /// <param name="Sum">Сумма</param>
        /// <returns>string - результат транзакции</returns>
        public void CreateTransaction(Transaction.TypeTransaction typeTransaction, Account FromAccount, Account ToAccount, double Sum)
        {
            if (typeTransaction == Transaction.TypeTransaction.DepositPercent) //если транзакция - начисление процентов по вкладу
            {
                Sum = (ToAccount as Deposit).CalculateInterest(Today); //считаем сумму начисленных процентов и если она не 0
                if (Sum != 0) //то создаем транзакцию
                {
                    Transaction transaction = new Transaction(typeTransaction, FromAccount, ToAccount, Sum); //создаем транзакцию
                    SQLAccountDB.UpdateAccount(ToAccount); //обновляем счет в БД
                    SQLTransactionDB.AddTransaction(transaction); //добавляем транзакцию в БД
                } 
            }
            else if (typeTransaction == Transaction.TypeTransaction.CreditPayment) //если транзакция - ежемесячный платеж по кредиту
            {
                Sum = (FromAccount as Credit).CalculateInterest(Today);//рассчитываем размер платежа и если не ноль
                if (Sum != 0)  //проводим платеж
                {
                    Transaction transaction = new Transaction(typeTransaction, FromAccount, ToAccount, Sum); //создаем транзакцию
                    SQLAccountDB.UpdateAccount(FromAccount); //обновляем счет в БД
                    SQLTransactionDB.AddTransaction(transaction); //добавляем транзакцию в БД
                } 
            }
            else //а во всех остальных случая сразу создаем транзакцию, в которой логика прописана внутри класса Transaction
            {
                Transaction transaction = new Transaction(typeTransaction, FromAccount, ToAccount, Sum); //создаем транзакцию
                if (FromAccount != null) SQLAccountDB.UpdateAccount(FromAccount); //если счет списания не нулл, то обновляем его в БД
                if (ToAccount != null) SQLAccountDB.UpdateAccount(ToAccount); //если счет зачисления не нулл, то обновляем его в БД
                SQLTransactionDB.AddTransaction(transaction); //добавляем транзакцию в БД
                UpdateAccountsColFromDB(); //обновляем коллекцию счетов банка из БД
                Transactions = SQLTransactionDB.GetAllTransactions(); //обновляем коллекцию транзакций банка из БД
            }
            
        }
        #endregion

        #region Методы работы со временем, начислением процентов
        /// <summary>
        /// Машина времени (чтобы не постареть в ожидании зачисления процентов по кредитам и вкладам)
        /// </summary>
        public void TimeMachine()
        {
            DateTime NextToday = Today.AddMonths(1); //временной переменной присваиваем текущую дату Today + месяц
            while (Today < NextToday) //увеличиваем значение Today до даты NextToday в цикле по одному дню
            {
                Today = Today.AddDays(1); //плюс день
                ChargeInterests(Accounts); //начисляем проценты по вкладам и кредитам
            }
            UpdateAccountsColFromDB(); //обновляем коллекцию счетов из БД
            Transactions = SQLTransactionDB.GetAllTransactions(); //обновляем коллекцию транзакций из БД
            SQLBankSettings.UpdateBankSettingsInDB(this); //обновляем свойства банка в БД (прежде всего Today)
        }

        /// <summary>
        /// Начисляет процента по вкладам и кредитам на текущую дату Today
        /// </summary>
        private void ChargeInterests(ObservableCollection<Account> accounts)
        {
            foreach (var account in accounts)
            {
                if (account is Deposit) CreateTransaction(Transaction.TypeTransaction.DepositPercent, null, account, 0);
                else if(account is Credit) CreateTransaction(Transaction.TypeTransaction.CreditPayment, account, null, 0);
            }
        }
        #endregion

        #region Методы, вызываемые событиями
        /// <summary>
        /// Добавляет новый элементы ActivityInfo и BalanceLog в БД
        /// </summary>
        /// <param name="sender">Отправитель события</param>
        /// <param name="e">Параметры активности(сообщение)</param>
        private void AddActivityToLogListAndBalanceLog(object sender, ActivityInfo e)
        {
            if (e.GetType() == typeof(ActivityInfo))
            {
                var send = sender as Account;
                e.Message = $"Счет Id:{send.Id} сообщил: {e.Message}";
                SQLActivityInfoDB.AddLogToDB(e);
                LogList = SQLActivityInfoDB.GetLog();
            }
            else if (e.GetType() == typeof(BalanceLog))
            {
                SQLBalanceLogDB.AddBalanceLogToDB(e as BalanceLog);
            }
        }
        #endregion

        /// <summary>
        /// Возвращает коллекцию BalanceLog из БД
        /// </summary>
        /// <param name="AccountId">Номер счета (Id)</param>
        /// <returns>List<BalanceLog></returns>
        public List<BalanceLog> GetBalanceLogs(int AccountId)
        {
            return SQLBalanceLogDB.GetLogCol(AccountId);
        }
    }
}
