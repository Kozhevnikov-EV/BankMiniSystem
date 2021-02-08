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
using System.Data.Entity;
using BankModel_Library;

namespace BankModel_Library
{
    public class Bank
    {
        #region События
        /// <summary>
        /// Событие "Исключение" возникает при исключении в экземпляре Bank
        /// </summary>
        public static event Action<Exception> BankException;

        /// <summary>
        /// Событие "Изменилась текущая дата Today"
        /// </summary>
        public static event Action<DateTime> TodayChanged;
        #endregion

        #region Поля и свойства
        /// <summary>
        /// Поле today, управляемое свойством Today
        /// </summary>
        private static DateTime today;

        /// <summary>
        /// Текущая дата банка
        /// </summary>
        public static DateTime Today 
        { 
            get { return today; }
            set { TodayChanged?.Invoke(value); today = value; } //при изменении вызываем событие "изменилась текущая дата"
        }

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
        /// Наименование банка
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Контекст настроек банка (Имя, текущая дата, базовая ставка)
        /// </summary>
        public BankSettingsContext Db_Settings { get; private set; }

        /// <summary>
        /// Контекст клиентов
        /// </summary>
        public ClientContext Db_Clients { get; private set; }

        /// <summary>
        /// Контекст счетов
        /// </summary>
        public AccountContext Db_Accounts { get; private set; }

        /// <summary>
        /// Контекст транзакций (операций по всем счетам)
        /// </summary>
        public TransactionContext Db_Transactions { get; private set; }

        /// <summary>
        /// Контекств с логами (общий лог последовательного изменения балансов счетов)
        /// </summary>
        public ActivitiesContext Db_Activities { get; set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Базовый конструктор, создает экземпляр банка из данных БД
        /// </summary>
        public Bank()
        {
            Db_Clients = new ClientContext(); //инициализируем все контексты
            Db_Accounts = new AccountContext();
            Db_Transactions = new TransactionContext();
            Db_Activities = new ActivitiesContext();
            Db_Settings = new BankSettingsContext();
            SubscribeBankItemsToEvenst(); //подписываем экземпляры классов клиентов и счетов на события
            //Если в контексте Db_Settings нет данных, то вызываем метод, устанавливающий значения по умолчанию
            if (Db_Settings.Settings.Find(1) == null) GetDefaultSettings();
            Bank.Today = Db_Settings.Settings.Find(1).Today; //присваиваем текущую дату из БД
            Bank.BaseRate = Db_Settings.Settings.Find(1).BaseRate; //присваиваем текущую базовую ставку из БД
            Name = Db_Settings.Settings.Find(1).Name; //присваиваем имя из БД
            Bank.TodayChanged += UpdateToday; //подписываем метод обновления текущей даты в БД и в классе на событие изменения даты
            InvalidCharException.CharCollection = Db_Settings.Settings.Find(1).IllegalCharsInName; //присваиваем недопустимые символы в наименовании
        }
        #endregion

        #region Вспомогательные методы, используемые при инициализации экземпляра класса через конструктор без параметров

        /// <summary>
        /// Метод изменения текущей даты Bank.Today
        /// </summary>
        /// <param name="newToday">Новая текущая дата</param>
        public void UpdateToday(DateTime newToday)
        {
            Db_Settings.Settings.Find(1).Today = newToday; //Обновляем дату в контексте
            Db_Settings.SaveChanges(); //сохраняем изменения в БД
        }

        /// <summary>
        /// Метод, устанавливающий свойства по умолнию в БД
        /// </summary>
        private void GetDefaultSettings()
        {
            Db_Settings.Settings.Add(new BankSettings { Id = 1, BaseRate = 10, Name = "Банк", Today = DateTime.Now });
            Db_Settings.SaveChanges();
        }

        /// <summary>
        /// Подписывает клиентов на событие "Закрытие кредита" (CreditFinished) их кредитных счетов (Credit) и подписывает счета
        /// на событие "Изменение в балансе" (Экземпляры классов клиентов и счетов загружаются из БД через контекст)
        /// </summary>
        /// <param name="bank">Экземпляр Bank, клиентов и счета которого надо подписывать на события</param>
        private void SubscribeBankItemsToEvenst()
        {
            foreach (var account in Db_Accounts.Accounts) //перебираем в цикле все счета
            {
                account.Activity += AddActivityToLogListAndBalanceLog; //и все счета подписываем на событие "Изменение баланса"
            }
            foreach (var credit in Db_Accounts.Credits) //перебираем все кредиты
            {
                var ClientWithCredit = Db_Clients.Clients.First(e => e.Id == credit.ClientId); //определяем владельца кредита
                credit.CreditFinished += ClientWithCredit.ChangeCreditRating; //подписываем его на событие закрытия кредита
                credit.CreditFinished += SaveClientChanges; //а также при закрытии кредита сохраняем изменения в БД из контекста
            }
        }
        #endregion

        #region Методы присвоения значений полям банка (Name, BaseRate) + ограничение на использование символов

        /// <summary>
        /// Устанавливает наименование банка, базовую процентную ставку и недопустимые символы в наименовании
        /// </summary>
        /// <param name="Name">Наименование банка</param>
        /// <param name="BaseRate">Базовая процентная ставка</param>
        /// <param name="IllegalChars">Неиспользуемые (запрещенные) символы</param>
        /// <returns>string - фактически присвоенные значения</returns>
        public string BankSettings(string Name, double BaseRate, params char[] IllegalChars)
        {
            BankSettings CurentSettings = Db_Settings.Settings.Find(1);
            try
            {
                CurentSettings.IllegalCharsInName = IllegalChars; //устанавливаем в свойство исключения набор недопустимых символов
                //проверяем введенное имя на наличие недопустимых символов
                if (Name.ContainsChars(InvalidCharException.CharCollection)) throw new InvalidCharException();
                //если находим - выбрасываем исключение
            }
            catch (Exception Ex)
            {
                BankException?.Invoke(Ex); //обрабатываем исключение
            }
            finally
            {
                Name = Name.TrimAllChars(InvalidCharException.CharCollection); //удаляем недопустимые символы из наименования
            }
            CurentSettings.Name = Name; //присваиваем имя, базовую ставку из параеметров метода
            CurentSettings.BaseRate = BaseRate;
            Db_Settings.SaveChanges();
            CurentSettings = Db_Settings.Settings.Find(1);
            Bank.BaseRate = CurentSettings.BaseRate;
            this.Name = CurentSettings.Name;
            return $"Наименоваение банка: {Db_Settings.Settings.Find(1).Name}. \nБазовая ставка: {Db_Settings.Settings.Find(1).BaseRate} %";
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
            Db_Clients.Naturals.Add(Natural); //добавляем в контекст
            Db_Clients.SaveChanges(); //сохранияем в БД
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
            VIP Vip = new VIP(Name, Surname, Birthday, WorkPlace); //создаем экземпляр ВИП клиента
            Db_Clients.VIPs.Add(Vip); //добавляем в контекст
            Db_Clients.SaveChanges(); //сохраняем в БД
        }

        /// <summary>
        /// Создает экземпляр юр. лица
        /// </summary>
        /// <param name="TypeOrg">Тип организации</param>
        /// <param name="Name">Наименование</param>
        public void AddCompany(string TypeOrg, string Name)
        {
            Company Company = new Company(TypeOrg, Name); //создаем экземпляр юр. лица
            Db_Clients.Companies.Add(Company); //добавляем в контекст
            Db_Clients.SaveChanges(); //сохраняем в БД
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
                var _client = Db_Clients.Naturals.FirstOrDefault(e => e.Id == client.Id);
                _client.Edit(Name, Surname, Birthday); //правим клиента
            }
            else if (client.GetType() == typeof(VIP)) //если клиент - VIP
            {
                var _client = Db_Clients.VIPs.FirstOrDefault(e => e.Id == client.Id);
                _client.Edit(Name, Surname, Birthday, WorkPlace); //правим клиента
            }
            else if (client.GetType() == typeof(Company)) //если клиент - юр.лицо
            {
                var _client = Db_Clients.Companies.FirstOrDefault(e => e.Id == client.Id);
                _client.Edit(TypeOrg, NameOrg); //правим клиента
            }
            Db_Clients.SaveChanges(); //сохраняем изменения в БД
        }
        #endregion

        #region Методы работы со счетами, процентными ставками
        /// <summary>
        /// Создает счет
        /// </summary>
        /// <param name="ClientId">Id клиента</param>
        public void AddAccount(int ClientId)
        {
            Account account = new Account(ClientId, 0); //добавляем счет, привязывая его к клиентскому Id
            account.Activity += AddActivityToLogListAndBalanceLog; //подписываем метод на событие изменения баланса
            Db_Accounts.Accounts.Add(account); //добавляем в контекст
            Db_Accounts.SaveChanges(); //сохраняем в БД
        }

        /// <summary>
        /// Закрывает счет
        /// </summary>
        /// <param name="AccountId">Id счета</param>
        public void CloseAccount(int AccountId)
        {
            Db_Accounts.Accounts.FirstOrDefault(e => e.Id == AccountId).IsOpen = false; //закрываем счет
            Db_Accounts.SaveChanges(); //сохраняем в БД
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
            deposit.Activity += AddActivityToLogListAndBalanceLog; //подписываем на событие
            Db_Accounts.Deposits.Add(deposit); //добавляем в контекст
            Db_Accounts.SaveChanges(); //сохраняем в БД
        }

        /// <summary>
        /// Добавляет Кредит указанному экземпляру Client
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="StartBalance">Стартовая сумма, внесенная клиентом на счет из личных средств</param>
        /// <param name="Percent">Процент по кредиту</param>
        /// <param name="CreditSum">Сумма кредита (зачисляется на кредитный счет и может быть снята клиентом)</param>
        /// <param name="duration">Срок кредита</param>
        public void AddCredit(Client client, double StartBalance, double Percent, double CreditSum, int duration)
        {
            DateTime EndDate = Today.AddMonths(duration); //Рассчитываем дату окончания кредита
            Credit credit = new Credit(client.Id, StartBalance, Percent, EndDate, CreditSum); //создаем экземпляр кредита
            var _client = Db_Clients.Clients.FirstOrDefault(e => e.Id == client.Id); //находим экземпляр клиента владельца в БД
            credit.CreditFinished += _client.ChangeCreditRating; //подписываем клиента на событие закрытия кредита
            credit.CreditFinished += SaveClientChanges; //а также при закрытии кредита сохраняем изменения в БД из контекста
            credit.Activity += AddActivityToLogListAndBalanceLog; //подписываем на событие изменения баланса
            Db_Accounts.Credits.Add(credit); //добавляем в контекст
            Db_Accounts.SaveChanges(); //сохраняем в БД
            Db_Clients.SaveChanges();
        }

        /// <summary>
        /// Сохраняет изменения контекста клиентов в БД (вызывается событием закрытия кредитов)
        /// </summary>
        /// <param name="plug"></param>
        private void SaveClientChanges(bool plug)
        {
            Db_Clients.SaveChanges();
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
            double Percent = BaseRate + client.CreditRating;
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
            double Percent = BaseRate - client.CreditRating;
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
                var toAccount_Db = Db_Accounts.Deposits.FirstOrDefault(e => e.Id == ToAccount.Id);
                Sum = toAccount_Db.CalculateInterest(Today); //считаем сумму начисленных процентов и если она не 0
                if (Sum != 0) //то создаем транзакцию
                {
                    Transaction transaction = new Transaction(typeTransaction, null, toAccount_Db, Sum); //создаем транзакцию
                    Db_Transactions.Transactions.Add(transaction); //добавляем в контекст
                    Db_Transactions.SaveChanges(); //сохраняем в БД
                }
            }
            else if (typeTransaction == Transaction.TypeTransaction.CreditPayment) //если транзакция - ежемесячный платеж по кредиту
            {
                var fromAccount_Db = Db_Accounts.Credits.FirstOrDefault(e => e.Id == FromAccount.Id); //присваиваем аккаунт с БД
                Sum = fromAccount_Db.CalculateInterest(Today);//рассчитываем размер платежа и если не ноль
                if (Sum != 0)  //проводим платеж
                {
                    Transaction transaction = new Transaction(typeTransaction, fromAccount_Db, null, Sum); //создаем транзакцию
                    Db_Transactions.Transactions.Add(transaction); //добавляем в контекст
                }
                Db_Transactions.SaveChanges(); //сохраняем в БД
            }
            else //а во всех остальных случая сразу создаем транзакцию, в которой логика прописана внутри класса Transaction
            {
                Account fromAccount_Db = null; //объявляем новые экземпляры счетов
                Account toAccount_Db = null;
                //и присваиваем им значения из БД
                if (FromAccount != null) fromAccount_Db = Db_Accounts.Accounts.FirstOrDefault(e => e.Id == FromAccount.Id);
                if(ToAccount != null) toAccount_Db = Db_Accounts.Accounts.FirstOrDefault(e => e.Id == ToAccount.Id);
                Transaction transaction = new Transaction(typeTransaction, fromAccount_Db, toAccount_Db, Sum); //создаем транзакцию
                Db_Transactions.Transactions.Add(transaction); //добавляем в контекст
                Db_Transactions.SaveChanges(); //сохраняем в БД
                Db_Accounts.SaveChanges();
            }

        }
        #endregion

        #region Методы работы со временем, начислением процентов
        /// <summary>
        /// Машина времени (чтобы не постареть в ожидании зачисления процентов по кредитам и вкладам)
        /// Делает списание/зачисление процентов, двигает дату пошагово
        /// </summary>
        public void TimeMachine()
        {
            DateTime NextToday = Today.AddMonths(1); //временной переменной присваиваем текущую дату Today + месяц
            while (Today < NextToday) //увеличиваем значение Today до даты NextToday в цикле по одному дню
            {
                Today = Today.AddDays(1); //плюс день
                ChargeInterests(); //начисляем проценты по вкладам и кредитам
            }
            Db_Accounts.SaveChanges(); //сохраняем изменения в БД по счетам
        }

        /// <summary>
        /// Начисляет процента по вкладам и кредитам на текущую дату Today
        /// </summary>
        private void ChargeInterests()
        {
            foreach (var account in Db_Accounts.Deposits)
            {
                CreateTransaction(Transaction.TypeTransaction.DepositPercent, null, account, 0);
            }
            foreach (var account in Db_Accounts.Credits)
            {
                CreateTransaction(Transaction.TypeTransaction.CreditPayment, account, null, 0);
            }
        }
        #endregion

        #region Методы, работающие с логами
        /// <summary>
        /// Добавляет новый элементы ActivityInfo и BalanceLog в БД
        /// </summary>
        /// <param name="sender">Отправитель события</param>
        /// <param name="e">Параметры активности(сообщение)</param>
        private void AddActivityToLogListAndBalanceLog(object sender, ActivityInfo e)
        {
            if (e.GetType() == typeof(ActivityInfo)) //если пришел экземпляр класса ActivityInfo
            {
                var send = sender as Account;
                e.Message = $"Счет Id:{send.Id} сообщил: {e.Message}";
                Db_Activities.ActivityInfos.Add(e); //добавляем в контекст
            }
            else if (e.GetType() == typeof(BalanceLog)) //если пришел экземпляр BalanceLog
            {
                Db_Activities.BalanceLogs.Add(e as BalanceLog); //добавляем в контекст
            }
            Db_Activities.SaveChanges(); //сохраняем в БД
        }
        #endregion

        #region Методы для View (подготовка коллекций счетов, клиентов, логов и транзакций для отображения в UI)
        /// <summary>
        /// Возвращает коллекцию BalanceLog из БД
        /// </summary>
        /// <param name="AccountId">Номер счета (Id)</param>
        /// <returns>List<BalanceLog></returns>
        public List<BalanceLog> GetBalanceLogs(int AccountId)
        {
            return Db_Activities.BalanceLogs.Where(e => e.AccountId == AccountId).ToList<BalanceLog>();
        }
        /// <summary>
        /// Наполняет переданную коллекцию clients экземплярами <T> из контекста Db_Clients
        /// </summary>
        /// <typeparam name="T">Унаследованный от Client класс</typeparam>
        /// <param name="clients">Коллекция клиентов, которую необходимо наполнить экземплярами T из БД</param>
        public void CreateClientCol<T>(ObservableCollection<T> clients)
            where T : Client
        {
            clients.Clear(); //очищаем коллекцию
            DbSet BankClientCollection = null; //объявляем экземпляр DbSet, в которую будем сохранять экземпляры клиентов класса T
            if (typeof(T) == typeof(NaturalPerson)) BankClientCollection = Db_Clients.Naturals;
            else if (typeof(T) == typeof(VIP)) BankClientCollection = Db_Clients.VIPs;
            else if (typeof(T) == typeof(Company)) BankClientCollection = Db_Clients.Companies;
            foreach (var client in BankClientCollection) //собираем коллекцию
            {
                clients.Add((T)client); //добавляем в нее экземпляры с типом T
            }
        }

        /// <summary>
        /// Возвращает коллекцию клиентов всех классов, унаследованных от Client
        /// </summary>
        /// <returns>List</returns>
        public List<Client> GetAllClients()
        {
            List<Client> clients = new List<Client>(); //инициализируем коллекцию
            List<Client> naturals = Db_Clients.Naturals.ToList<Client>(); //создаем коллекции клиентов из БД
            List<Client> vips = Db_Clients.VIPs.ToList<Client>();
            List<Client> companies = Db_Clients.Companies.ToList<Client>();
            clients = clients.Concat(naturals).Concat(vips).Concat(companies).ToList<Client>(); //объединяем коллекции клиентов
            return clients;
        }

        /// <summary>
        /// Наполняет переданную коллекцию account экземплярами <T> из контекста Db_Accounts, принадлежащих client
        /// </summary>
        /// <typeparam name="T">Унаследованный от Account класс</typeparam>
        /// <param name="accounts">Коллекция счетов, которую необходимо наполнить</param>
        /// <param name="client">Экземпляр клиента, для которого необходимо найти все счета</param>
        public void CreateAccountCol<T>(ObservableCollection<T> accounts, Client client)
            where T : Account
        {
            if (client != null)
            {
                accounts?.Clear(); //очищаем переданную коллекцию
                List<Account> BankAccountsCollection = null; //объявляем новую коллекцию счетов
                //собираем в коллекцию List
                if (typeof(T) == typeof(Account)) BankAccountsCollection = Db_Accounts.Accounts.Where(e => e.ClientId == client.Id).ToList<Account>();
                else if (typeof(T) == typeof(Deposit)) BankAccountsCollection = Db_Accounts.Deposits.Where(e => e.ClientId == client.Id).ToList<Account>();
                else if (typeof(T) == typeof(Credit)) BankAccountsCollection = Db_Accounts.Credits.Where(e => e.ClientId == client.Id).ToList<Account>();
                foreach (var account in BankAccountsCollection) //и перемещаем в ObservableCollection
                {
                    accounts.Add((T)account);
                }
            }
        }

        /// <summary>
        /// Наполняет переданную коллекцию account экземплярами из коллекции bank.accounts, принадлежащих client
        /// Имеющих статус Open и без классов наследников (без кредитов и вкладов)
        /// </summary>
        /// <param name="bank">Экземпляр банка, хранящий все счета в коллекции bank.accounts</param>
        /// <param name="accounts">Коллекция счетов, которую необходимо наполнить</param>
        /// <param name="client">Экземпляр клиента, для которого необходимо найти все открытые счета</param>
        public void CreateOpenAccountCol(ObservableCollection<Account> accounts, Client client)
        {
            if (client != null) //проверяем переданного клиента на нул
            {
                accounts?.Clear(); //очищаем переданную коллекцию
                var OpenAccounts = Db_Accounts.Accounts.Where(e => e.ClientId == client.Id && e.IsOpen); //формируем запрос в контекст
                foreach (var account in OpenAccounts) //наполняем коллекцию
                {
                    accounts.Add(account);
                }
            }
        }

        /// <summary>
        /// Возвращает коллекцию всех транзакций по счету account
        /// </summary>
        /// <param name="account">Счет, по которому необходимо вывести все транзакции</param>
        /// <returns></returns>
        public ObservableCollection<Transaction> GetTransactionsOfAccount(Account account)
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
            foreach (var transaction in Db_Transactions.Transactions.Where(e => e.FromAccount == account.Id || e.ToAccount == account.Id))
            {
                transactions.Add(transaction);
            }
            return transactions;
        }

        /// <summary>
        /// Наполняет переданную коллекцию activities экземплярами ActivityInfo из БД с сортировкой по Id
        /// </summary>
        /// <param name="activities"></param>
        public void GetLogCollection(ObservableCollection<ActivityInfo> activities)
        {
            activities?.Clear(); //очищаем переданную коллекцию
            var _activities = from i in Db_Activities.ActivityInfos //формируем LINQ запрос
                              orderby i.Id descending
                              select i;
            foreach (var act in _activities) //из возвращенного результата отбираем экземпляры ActivityInfo и добавляем в коллекцию
            {
                if(act.GetType() == typeof(ActivityInfo)) activities.Add(act);
            }
        }

        /// <summary>
        /// Возвращает коллекцию всех транзакций
        /// </summary>
        /// <returns>ObservableCollection<Transaction></returns>
        public ObservableCollection<Transaction> GetAllTransactions()
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>(); //инициализируем коллекцию
            foreach(var transaction in Db_Transactions.Transactions) //пробегаемся по контексту транзакций циклом
            {
                transactions.Add(transaction); //добавляем транзакции в коллекцию
            }
            return transactions; //возвращаем коллекцию транзакций
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров ActivityInfo без сортировки
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ActivityInfo> GetAllActivities()
        {
            ObservableCollection<ActivityInfo> activities = new ObservableCollection<ActivityInfo>(); //инициализируем коллекцию
            foreach (var activity in Db_Activities.ActivityInfos) //в цикле пробегаемся по контексту
            {
                if (activity.GetType() == typeof(ActivityInfo)) activities.Add(activity); //добавляем в коллекцию экземпляры ActivityInfo
            }
            return activities; //возвращаем коллекцию ActivityInfo
        }
        #endregion
    }
}

