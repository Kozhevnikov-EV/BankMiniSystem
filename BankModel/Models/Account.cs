using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    /// <summary>
    /// Супер-класс Счет
    /// </summary>
    public class Account
    {
        #region Статичное поле, конструктор и методы для получения Id экземпляра Account и сброса значения staticId
        /// <summary>
        /// Статическое поле для staticId
        /// </summary>
        [JsonProperty]
        private static int staticId;

        /// <summary>
        /// Статичный конструктор для инициализации staticId
        /// </summary>
        static Account()
        {
            staticId = 0;
        }

        /// <summary>
        /// Статический метод для получения следущего Id
        /// </summary>
        /// <returns>Id</returns>
        private static int NextId()
        {
            staticId++;
            return staticId;
        }

        /// <summary>
        /// Метод сброса текущего значения staticId
        /// </summary>
        public static void ResetStaticId()
        {
            staticId = 0;
        }
        #endregion

        #region Поля и свойства класса + Событие!
        /// <summary>
        /// Событие "Изменение баланса" - активность счета
        /// </summary>
        public event Action<object, ActivityInfo> Activity;

        /// <summary>
        /// Тип(имя) счета
        /// </summary>
        public virtual string Name => "Счет";

        /// <summary>
        /// Номер счета
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Id владельца счета
        /// </summary>
        public int ClientId { get; }

        /// <summary>
        /// Поле статус счета (открыт - true)
        /// </summary>
        protected bool isOpen;

        /// <summary>
        /// Свойство статус счета (открыт - true). Может быть закрыт лишь единожды
        /// </summary>
        public bool IsOpen 
        { 
            get { return isOpen; } 
            ///Закрытие счета только при 0 балансе. При закрытии - запрет на снятие и пополнение
            set { if (value == false && balance == 0) { isOpen = false; IsRefill = false; IsWithdrawal = false; } else { isOpen = true; } } 
        }

        /// <summary>
        /// Текстовое представление текущего состояния поля isOpen
        /// </summary>
        public string TextStatus { get { return isOpen ? "Открыт" : "Закрыт"; } }

        /// <summary>
        /// Свойство - пополняемость счета (true - счет пополняемый)
        /// </summary>
        public bool IsRefill { get; set; }

        /// <summary>
        /// Текстовый статус свойства IsRefill
        /// </summary>
        public string TextIsRefill { get { return IsRefill ? "Доступно" : "Недоступно"; } }

        /// <summary>
        /// Свойство - доступность снятия (true - снятие средств со счета доступно)
        /// </summary>
        public bool IsWithdrawal { get; set; }

        /// <summary>
        ///  Текстовый статус свойства IsWithdrawal
        /// </summary>
        public string TextIsWithdrawal { get { return IsWithdrawal ? "Доступно" : "Недоступно"; } }

        /// <summary>
        /// Дата открытия вклада
        /// </summary>
        public DateTime OpenDate { get; }

        /// <summary>
        /// Поле текущий баланс счета
        /// </summary>
        protected double balance;

        /// <summary>
        /// Свойство текущий баланс счета (с округлением до сотых)
        /// </summary>
        public virtual double Balance 
        { 
            get { return balance; } 
            ///Баланс не может отрицательным
            set { if (isOpen == true && value >= 0) { balance = Math.Round(value, 2); } } 
        }
        
        /// <summary>
        /// Коллекция BalanceLog - история значений баланса счета
        /// </summary>
        [JsonProperty]
        public List<BalanceLog> Logs { get; private set; }

        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        protected Account(int Id, int ClientId, bool IsOpen, bool IsRefill, bool IsWithdrawal, DateTime OpenDate, double Balance,
            List<BalanceLog> Logs)
        {
            this.Id = Id;
            this.ClientId = ClientId;
            isOpen = IsOpen;
            this.IsRefill = IsRefill;
            this.IsWithdrawal = IsWithdrawal;
            this.OpenDate = OpenDate;
            this.balance = Balance;
            this.Logs = new List<BalanceLog>();
            this.Logs = Logs;
        }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="ClientId">Id клиента - владельца счета</param>
        /// <param name="StartBalance">Стартовая сумма на счете</param>
        public Account(int ClientId, double StartBalance)
        {
            Id = NextId();
            isOpen = true;
            IsRefill = true;
            IsWithdrawal = true;
            this.ClientId = ClientId;
            Balance = Math.Round(StartBalance, 2);
            OpenDate = Bank.Today;
            Logs = new List<BalanceLog>(); //{ new BalanceLog(Balance, 0) };
        }
        #endregion

        #region Методы
        /// <summary>
        /// Зачисление суммы на счет
        /// </summary>
        /// <param name="Sum">Сумма</param>
        /// <returns>bool - результат операции</returns>
        public bool Refill(double Sum, int TransactionId)
        {
            bool Success = false; //контрольная переменная
            double StartBalance = balance; //переменная со значением баланса до его изменения
            Balance += Sum; //пополняем баланс
            if (StartBalance == Math.Round((Balance - Sum), 2)) { Success = true; } //проверяем баланс после пополнения
            //Создаем экземпляр класса ActivityInfo с информацией об изменении баланса
            ActivityInfo activity = new ActivityInfo($"Баланс изменился: {StartBalance} --> {Balance} (Транзакция Id {TransactionId})");
            Activity?.Invoke(this, activity); //вызываем событие - т.е. выполняем подписанные на события методы соответвующих экземпляров классов
            Logs.Add(new BalanceLog(Balance, TransactionId)); //добавляем информацию о балансе и транзакции в логи
            return Success; //возвращаем контрольную переменную
        }

        /// <summary>
        /// Списание суммы со счета
        /// </summary>
        /// <param name="Sum">Сумма</param>
        /// <returns>bool - результат операции</returns>
        public bool Withdrawal(double Sum, int TransactionId)
        {
            bool Success = false; //контрольная переменная
            double StartBalance = balance; //переменная со значением баланса до его изменения
            Balance -= Sum;//уменьшаем баланс
            if (StartBalance == Math.Round((Balance + Sum), 2)) { Success = true; } //проверяем баланс после пополнения
            //Создаем экземпляр класса ActivityInfo с информацией об изменении баланса
            ActivityInfo activity = new ActivityInfo($"Баланс изменился: {StartBalance} --> {Balance} (Транзакция Id {TransactionId})");
            Activity?.Invoke(this, activity); //вызываем событие - т.е. выполняем подписанные на события методы соответвующих экземпляров классов
            Logs.Add(new BalanceLog(Balance, TransactionId)); //добавляем информацию о балансе и транзакции в логи
            return Success; //возвращаем контрольную переменную
        }
        #endregion

        #region Перегрузка операторов ">" и "<"

        /// <summary>
        /// Перегрузка оператора > для двух экземпляров Account (сравнение по балансу)
        /// </summary>
        /// <param name="account1"></param>
        /// <param name="account2"></param>
        /// <returns>true - баланс account1 больше баланса account2
        /// false - меньше или равен</returns>
        public static bool operator >(Account account1, Account account2)
        {
            return account1.Balance > account2.Balance;
        }

        /// <summary>
        /// Перегрузка оператора > для двух экземпляров Account (сравнение по балансу)
        /// </summary>
        /// <param name="account1"></param>
        /// <param name="account2"></param>
        /// <returns>true - баланс account1 больше баланса account2
        /// false - меньше или равен</returns>
        public static bool operator <(Account account1, Account account2)
        {
            return account1.Balance < account2.Balance;
        }
        #endregion
    }
}
