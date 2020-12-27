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
        public event Action<Exception> BankException;

        /// <summary>
        /// Отображает текущую дату
        /// </summary>
        [JsonProperty]
        public static DateTime Today { get; private set; }

        /// <summary>
        /// Базовая ставка - поле
        /// </summary>
        [JsonProperty]
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
        public string Name { get; private set; }

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
        /// Базовый конструктор, создает пустой экземпляр Bank
        /// </summary>
        public Bank()
        {
            Today = DateTime.Now;
            Clients = new ObservableCollection<Client>();
            Accounts = new ObservableCollection<Account>();
            Transactions = new ObservableCollection<Transaction>();
            LogList = new ObservableCollection<ActivityInfo>();
        }

        /// <summary>
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        private Bank(string Name)
            : this()
        {
            this.Name = Name;
        }

        /// <summary>
        /// Переопределенный конструктор с объявлением свойств Name и BaseRate
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="BaseRate">Базова процентная ставка</param>
        public Bank(string Name, int BaseRate)
            : this()
        {
            this.Name = Name;
            Bank.BaseRate = BaseRate;
        }
        #endregion

        #region Индексаторы

        /// <summary>
        /// Возвращает экземпляр клиента которому принадлежит счет
        /// </summary>
        /// <param name="account">счет</param>
        /// <returns>Client</returns>
        public Client this[Account account]
        {
            get
            {
                Client temp = null;
                foreach (var e in Clients)
                {
                    if (e.Id == account.ClientId) { temp = e; break; }
                }
                return temp;
            }
        }
        #endregion

        #region Методы работы с Json и подписка на события десериазизованного экземпляра Bank!
        public Bank LoadFromJson(string Path)
        {
            Bank bank = null;
            Account.ResetStaticId();
            Client.ResetStaticId();
            Transaction.ResetStaticId();
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }; //в переменную с настройками для Json конвертера
            var json = "";                                                                        //прописываем запись всех .NET типов имен (чтобы конвертер знал какой объект он конвертирует)
            try
            {
                json = File.ReadAllText(Path); //считываем файл в текстовую переменную
                bank = JsonConvert.DeserializeObject<Bank>(json, settings); //десериализуем
                SubscribeBankItemsToEvenst(bank); //вызываем метод "оформления подписок"
            }
            // !!!Убрал обработку данного исключения из этого участка кода, т.к обработкой исключений теперь занимается UI, который
            // подписан на событие BankException !!!
            //catch (FileNotFoundException Ex) //обрабатываем исключение если файл не найден
            //{
            //    BankException?.Invoke(Ex);
            //}
            //catch (JsonException Ex) //исключение, если ошибка произошла в ходе десериализации
            //{
            //    BankException?.Invoke(Ex);
            //}
            catch (Exception Ex) //Прочие исключения
            {
                //ExceptionMessageBox Box = new ExceptionMessageBox(Ex);  //выводим MessageBox на экран
                BankException?.Invoke(Ex);
            }
            if (bank == null) //если не удалось загрузить банк с файла, то создаем новый
            {
                bank = new Bank("Новый банк", 12);
            }
            return bank;
        }

        /// <summary>
        /// Подписывает клиентов на событие "Закрытие кредита" (CreditFinished) их кредитных счетов (Credit) и подписывает счета
        /// на событие "Изменение в балансе"
        /// Используется при загрузке с Json
        /// </summary>
        /// <param name="bank">Экземпляр Bank, клиентов и счета которого надо подписывать на события</param>
        private void SubscribeBankItemsToEvenst(Bank bank)
        {
            foreach(var account in bank.Accounts) //перебираем в цикле все счета
            {
                account.Activity += bank.AddActivityToLogList; //и все счета подписываем на событие "Изменение баланса"
                if(account is Credit) //если натыкаемся на кредит
                {
                    var clients = bank.Clients.Where(e => e.Id == account.ClientId); //то находим клиента - владельца кредита
                    foreach(var client in clients)
                    {
                        (account as Credit).CreditFinished += client.ChangeCreditRating; //и подписываем его на событие закрытия кредита
                    }
                }
            }
        }

        public void SaveToJson(string Path)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All,  }; //в переменную с настройками для Json конвертера
            //прописываем запись всех .NET типов имен (чтобы конвертер знал какой объект он конвертирует)
            var json = JsonConvert.SerializeObject(this, settings); //сериализуем
            File.WriteAllText(Path, json); //пишем в файл
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
            // !!!Убрал обработку данного исключения из этого участка кода, т.к обработкой исключений теперь занимается UI, который
            // подписан на событие BankException !!!
            //catch (InvalidCharException Ex)
            //{
            //    ExceptionMessageBox Box = new ExceptionMessageBox(Ex);
            //    BankException?.Invoke(Ex);
            //} 
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
            Client Natural = new NaturalPerson(Name, Surname, Birthday); //создаем экземпляр физ лица
            Clients.Add(Natural); //добавляем в коллекцию
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
            Client VIP = new VIP(Name, Surname, Birthday, WorkPlace); //создаем экземпляр ВИП клиента
            Clients.Add(VIP); //добавляем в коллекцию
        }

        /// <summary>
        /// Создает экземпляр юр. лица
        /// </summary>
        /// <param name="TypeOrg">Тип организации</param>
        /// <param name="Name">Наименование</param>
        public void AddCompany(string TypeOrg, string Name)
        {
            Client Company = new Company(TypeOrg, Name); //создаем экземпляр юр. лица
            Clients.Add(Company); //добавляем в коллекцию
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
            if (client.GetType() == typeof(NaturalPerson)) { (client as NaturalPerson).Edit(Name, Surname, Birthday); }
            else if (client.GetType() == typeof(VIP)) { (client as VIP).Edit(Name, Surname, Birthday, WorkPlace); }
            else if (client.GetType() == typeof(Company)) { (client as Company).Edit(TypeOrg, NameOrg); }
        }
        #endregion

        #region Методы работы со счетами + подписка счетов на события
        /// <summary>
        /// Создает счет
        /// </summary>
        /// <param name="ClientId">Id клиента</param>
        public void AddAccount(int ClientId, double Sum)
        {
            Sum = Math.Round(Sum, 2);
            Account account = new Account(ClientId, 0); //добавляем счет, привязывая его к клиентскому Id
            account.Activity += this.AddActivityToLogList; //подписываем счет на событие Изменение баланса
            Accounts.Add(account);
        }

        /// <summary>
        /// Закрывает счет
        /// </summary>
        /// <param name="AccountId">Id счета</param>
        public void CloseAccount(int AccountId)
        {
            foreach (var e in Accounts) //ищем счет в коллекции и закрываем его
            {
                if (e.Id == AccountId && e.IsOpen == true) { e.IsOpen = false; break; }
            }
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
            deposit.Activity += this.AddActivityToLogList; //подписываем счет на событие Изменение баланса
            Accounts.Add(deposit); //добавляем его в коллекцию всех счетов
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
            credit.Activity += this.AddActivityToLogList; //подписываем счет на событие Изменение баланса - регистрируем AсtivityInfo
            credit.CreditFinished += client.ChangeCreditRating; //подписываем счет на событие Закрытие кредита - редактируем кредитный рейтинг
            Accounts.Add(credit); //добавляем в коллекцию всех счетов
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
                if (Sum != 0) { Transactions.Add(new Transaction(typeTransaction, FromAccount, ToAccount, Sum)); } //то создаем транзакцию
            }
            else if (typeTransaction == Transaction.TypeTransaction.CreditPayment) //если транзакция - ежемесячный платеж по кредиту
            {
                Sum = (FromAccount as Credit).CalculateInterest(Today);//рассчитываем размер платежа и если не ноль
                if (Sum != 0) { Transactions.Add(new Transaction(typeTransaction, FromAccount, ToAccount, Sum)); } //проводим платеж
            }
            else //а во всех остальных случая сразу создаем транзакцию, в которой логика прописана внутри класса Transaction
            {
                Transactions.Add(new Transaction(typeTransaction, FromAccount, ToAccount, Sum));
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
                ChargeInterests(); //начисляем проценты по вкладам и кредитам
            }
        }

        /// <summary>
        /// Начисляет процента по вкладам и кредитам на текущую дату Today
        /// </summary>
        private void ChargeInterests()
        {
            foreach (var account in Accounts) //в цикле перебираем все счета в банке
            {
                if (account is Deposit) //если находим вклад, то начиcляем проценты по вкладу (создаем транзакцию)
                {
                    this.CreateTransaction(Transaction.TypeTransaction.DepositPercent, null, account, 0);
                }
                else if (account is Credit) //аналогично, если находим кредит
                {
                    this.CreateTransaction(Transaction.TypeTransaction.CreditPayment, account, null, 0);
                }
            }
        }
        #endregion

        #region Методы, вызываемые событиями
        /// <summary>
        /// Добавляет новый элемент ActivityInfo в коллекцию LogList
        /// </summary>
        /// <param name="sender">Отправитель события</param>
        /// <param name="e">Параметры активности(сообщение)</param>
        private void AddActivityToLogList(object sender, ActivityInfo e)
        {
            var send = sender as Account;
            e.Message = $"Счет Id:{send.Id} сообщил: {e.Message}";
            LogList.Add(e);
        }
        #endregion
    }
}
