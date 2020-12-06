using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    /// <summary>
    /// Класс, наследник ActivityInfo, экземпляры которого отображают состояние баланса экземпляра Account на дату
    /// </summary>
    public class BalanceLog : ActivityInfo, IComparable<BalanceLog>, ICloneable
    {
        #region Свойства
        /// <summary>
        /// Баланс
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// Id транзакции, на основании которой баланс счета принял значение Balance
        /// </summary>
        public int TransactionId { get; }

        /// <summary>
        /// Переопределенное свойство класса ActivityInfo. В данном классе содержит тектовое значение свойтсва Balance
        /// </summary>
        public override string Message { get => base.Message; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="balance">Баланс</param>
        /// <param name="transationId">Id транзакции</param>
        public BalanceLog(double balance, int transationId)
            : base(balance.ToString())
        {
            Balance = balance;
            TransactionId = transationId;
        }

        /// <summary>
        /// Конструктор, позволяющий принудительно установить любое значение Date(в рамках DateTime)
        /// Используется для создания копий с подменой некоторых полей
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="balance">Баланс</param>
        /// <param name="transationId">Id транзакции</param>
        [JsonConstructor]
        public BalanceLog(DateTime date, double balance, int transationId)
            : base(date, balance.ToString())
        {
            Balance = balance;
            TransactionId = transationId;
        }
        #endregion

        #region Реализация методов интерфейсов
        /// <summary>
        /// Компаратор (реализация IComparable)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(BalanceLog other)
        {
            if (this.Balance > other.Balance) return 1;
            else if (this.Balance < other.Balance) return -1;
            else return 0;
        }

        /// <summary>
        /// Копирует экзепляр BalanceLog (реализация интерфейса IClonable)
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            BalanceLog copy = new BalanceLog(Date, Balance, TransactionId);
            return copy;
        }
        #endregion
    }
}
