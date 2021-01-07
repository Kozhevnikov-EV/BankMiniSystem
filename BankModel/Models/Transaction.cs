using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    /// <summary>
    /// Класс Транзакция
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Enum с типами возможных транзакций
        /// </summary>
        public enum TypeTransaction
        {
            CashRefill,
            CashWithdrawal,
            CashlessTransfer,
            DepositPercent,
            CreditPayment
        }

        /// <summary>
        /// Делегат - обработчик транзакции, содержит методы работы со счетами для конкретного экземпляра Transaction
        /// </summary>
        [JsonIgnore]
        public Func<double, int, bool> TransactionHandler;

        #region Статичное поле, конструктор и метод для получения Id экземпляра Transaction
        /// <summary>
        /// Статическое поле для staticId
        /// </summary>
        [JsonProperty]
        private static int staticId;

        /// <summary>
        /// Статичный конструктор для инициализации staticId
        /// </summary>
        static Transaction()
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

        public static void ResetStaticId()
        {
            staticId = 0;
        }
        #endregion

        #region Поля и свойства
        /// <summary>
        /// Индивидуальный номер транзакции
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Наименование операции
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Дата и время выполнения транзакции
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Id счета отправителя
        /// </summary>
        public int FromAccount { get; private set; }

        /// <summary>
        /// Id счета получателя
        /// </summary>
        public int ToAccount { get; private set; }

        /// <summary>
        /// Сумма перевода
        /// </summary>
        public double Sum { get; private set; }

        public bool Status { get; private set; }

        public string TextStatus { get { return Status ? "Выполнена" : "Не выполнена"; } }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор для Json + SQL
        /// </summary>
        [JsonConstructor]
        public Transaction(int Id, string Name, DateTime Date, int FromAccount, int ToAccount, double Sum, bool Status)
        {
            this.Id = Id;
            this.Name = Name;
            this.Date = Date;
            this.FromAccount = FromAccount;
            this.ToAccount = ToAccount;
            this.Sum = Sum;
            this.Status = Status;
        }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="typeTransaction">Enum транзакции</param>
        /// <param name="fromAccount">Id счета отправителя</param>
        /// <param name="toAccount">Id счета получателя</param>
        /// <param name="sum">Сумма перевода</param>
        public Transaction(TypeTransaction typeTransaction, Account fromAccount, Account toAccount, double sum)
        {
            Id = NextId();
            Date = Bank.Today;
            Sum = Math.Round(sum, 2);
            Status = false;
            GetNameAndHandler(typeTransaction, fromAccount, toAccount); //вызываем метод, присваивающий имена и подписывающий методы на событие
            TransactionHandler?.Invoke(sum, Id); //вызываем событие
        }
        #endregion

        #region Методы
        /// <summary>
        /// Метод присваивающий значения свойству Name и присваивающий делегату
        /// TransactionHandler адреса методов Refill и Withdrawal счетов, учавствующих в транзакции
        /// на событие TransactionHandler
        /// </summary>
        /// <param name="typeTransaction">Enum транзакции</param>
        /// <param name="fromAccount">Id счета отправителя</param>
        /// <param name="toAccount">Id счета получателя</param>
        private void GetNameAndHandler(TypeTransaction typeTransaction, Account fromAccount, Account toAccount)
        {
            switch (typeTransaction) //в записимости от выбранного Enum вызываем соответвующий метод
            {
                case TypeTransaction.CashRefill:
                    CashRefillTransaction(toAccount);
                    break;
                case TypeTransaction.CashWithdrawal:
                    CashWithdrawalTransaction(fromAccount);
                    break;
                case TypeTransaction.CashlessTransfer:
                    CashlessTransferTransaction(fromAccount, toAccount);
                    break;
                case TypeTransaction.DepositPercent:
                    DepositPercentTransaction(toAccount);
                    break;
                case TypeTransaction.CreditPayment:
                    CreditPaymentTransaction(fromAccount);
                    break;
            }
        }

        /// <summary>
        /// Пополнение наличными: Присваивает Name и присваивает делегату TransactionHandler адреcа методов для совершения транзакции
        /// </summary>
        /// <param name="toAccount"></param>
        private void CashRefillTransaction(Account toAccount)
        {
            Name = "Пополнение наличными";
            ToAccount = toAccount.Id;
            if (toAccount.IsOpen && toAccount.IsRefill) { TransactionHandler += toAccount.Refill; Status = true; }
        }

        /// <summary>
        /// Снятие наличных: Присваивает Name и присваивает делегату TransactionHandler адреcа методов для совершения транзакции
        /// </summary>
        /// <param name="fromAccount"></param>
        private void CashWithdrawalTransaction(Account fromAccount)
        {
            Name = "Снятие наличных";
            FromAccount = fromAccount.Id;
            if(fromAccount.IsOpen && fromAccount.IsWithdrawal && fromAccount.Balance >= Sum) 
            { TransactionHandler += fromAccount.Withdrawal; Status = true; }
        }

        /// <summary>
        /// Перевод средств: Присваивает Name и присваивает делегату TransactionHandler адреcа методов для совершения транзакции
        /// </summary>
        /// <param name="fromAccount"></param>
        /// <param name="toAccount"></param>
        private void CashlessTransferTransaction(Account fromAccount, Account toAccount)
        {
            Name = "Перевод средств";
            FromAccount = fromAccount.Id;
            ToAccount = toAccount.Id;
            if(fromAccount.IsOpen && fromAccount.IsWithdrawal && fromAccount.Balance >= Sum && toAccount.IsOpen && toAccount.IsRefill)
            {
                TransactionHandler += fromAccount.Withdrawal;
                TransactionHandler += toAccount.Refill;
                Status = true;
            }
        }

        /// <summary>
        /// Начисление процентов по вкладу: Присваивает Name и присваивает делегату TransactionHandler адреcа методов для совершения транзакции
        /// </summary>
        /// <param name="toAccount"></param>
        private void DepositPercentTransaction(Account toAccount)
        {
            Name = "Начисление процентов по вкладу";
            ToAccount = toAccount.Id;
            if (toAccount is Deposit && toAccount.IsOpen) 
            { TransactionHandler += toAccount.Refill; Status = true; }
        }

        /// <summary>
        /// Ежемесячный платеж по кредиту: Присваивает Name и присваивает делегату TransactionHandler адреcа методов для совершения транзакции
        /// </summary>
        /// <param name="fromAccount"></param>
        private void CreditPaymentTransaction(Account fromAccount)
        {
            Name = "Ежемесячный платеж по кредиту";
            FromAccount = fromAccount.Id;
            if (fromAccount.IsOpen && fromAccount is Credit)
            { TransactionHandler += fromAccount.Withdrawal; Status = true; }
        }
        #endregion
    }
}
