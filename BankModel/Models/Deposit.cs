using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    /// <summary>
    /// Класс Вклад (наследник Account)
    /// </summary>
    public class Deposit : Account, IAccount
    {
        #region Поля и свойства
        /// <summary>
        /// Имя (тип) (переопределенное свойство)
        /// </summary>
        public override string Name => "Вклад" ;

        /// <summary>
        /// Установленный процент по вкладу (годовой)
        /// </summary>
        public double Percent { get; private set; }

        /// <summary>
        /// Ежемесячная капитализация
        /// </summary>
        public bool Capitalization { get; private set; }

        /// <summary>
        /// Текстовый статус капитализации
        /// </summary>
        public string TextCapitalization { get { return Capitalization ? "Да" : "Нет"; } }

        /// <summary>
        /// Дата окончания срока действия
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Дата предыдущей капитализации (только для вкладов с капитализацией)
        /// </summary>
        public DateTime PreviousCapitalization { get; set; }

        /// <summary>
        /// Даты капитализации процентов по вкладу (только для вкладов с капитализацией)
        /// </summary>
        public Queue<DateTime> CapitalizationDates { get; private set; }

        /// <summary>
        /// Поле текущий статус вклада
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Свойствок текущий статус вклада (true - проценты начисляются)
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                //При окончании срока действия вклада возможно снятие средств со счета
                isActive = value;
                if (value) { IsRefill = false; IsWithdrawal = false; } else { IsRefill = false; IsWithdrawal = true; }
            }
        }

        /// <summary>
        /// Текстовый статус вклада (Да - проценты начисляются)
        /// </summary>
        public string TextIsActive { get { return IsActive ? "Да" : "Нет"; } }

        /// <summary>
        /// Текущий Баланс счета (переопределенное свойство)
        /// </summary>
        public override double Balance
        {
            get { return base.balance; }
            set
            {
                if (isOpen == true && value > 0)
                {
                    balance = Math.Round(value, 2);
                }
                else if (isOpen == true && isActive == false && value == 0) //после окончания срока действия при балансе 0 - закрываем счет
                {
                    balance = 0;
                    base.IsOpen = false; //обращаемся к свойству класса-родителя
                }
            }
        }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Основной конструктор для создания экземпляра класса Deposit (без присвоения Id)
        /// </summary>
        /// <param name="clientId">Id клиента - владельца</param>
        /// <param name="startBalance">Стартовая сумма на счете</param>
        /// <param name="percent">Процент</param>
        /// <param name="capitalization">Капитализация</param>
        /// <param name="endDate">Дата окончания</param>
        public Deposit(int clientId, double startBalance, double percent, bool capitalization, DateTime endDate)
            : base(clientId, startBalance)
        {
            //Логика установки процентной ставки
            this.Percent = (percent <= 0 || percent > 100) ? this.Percent = Bank.BaseRate : Math.Round(percent, 2);
            //Далее присваеваем свойства экземпляра класса из параметров, переданных в конструктор
            Capitalization = capitalization;
            EndDate = endDate;
            IsActive = true;
            IsRefill = false;
            if(Capitalization) //отдельная логика, если класс с капитализацией процентов
            {
                PreviousCapitalization = OpenDate;
                CapitalizationDates = new Queue<DateTime>(); //заполняем коллекцию датами капитализации процентов по вкладу
                DateTime NextCapitalization = OpenDate;
                do
                {
                    NextCapitalization = NextCapitalization.AddMonths(1);
                    CapitalizationDates.Enqueue(NextCapitalization);
                } while (NextCapitalization <= EndDate);
            }
        }

        /// <summary>
        /// Конструктор для SQL (прямое присвоение Id из БД)
        /// </summary>
        public Deposit(int Id, int ClientId, bool IsOpen, bool IsRefill, bool IsWithdrawal, DateTime OpenDate, double Balance, double Percent,
          bool Capitalization, DateTime EndDate, DateTime PreviousCapitalization, bool IsActive)
            : base(Id, ClientId, IsOpen, IsRefill, IsWithdrawal, OpenDate, Balance)
        {
            this.Percent = Percent;
            this.Capitalization = Capitalization;
            this.EndDate = EndDate;
            this.PreviousCapitalization = PreviousCapitalization;
            this.CapitalizationDates = new Queue<DateTime>();
            DateTime NextCapitalization = PreviousCapitalization;
            do
            {
                NextCapitalization = NextCapitalization.AddMonths(1);
                CapitalizationDates.Enqueue(NextCapitalization);
            } while (NextCapitalization <= EndDate);
            this.IsActive = IsActive;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Метод расчета суммы начисленных процентов по вкладу
        /// </summary>
        /// <param name="dateTime">Текущая дата</param>
        /// <returns>Сумму начисленных процентов за период</returns>
        public double CalculateInterest(DateTime dateTime)
        {
            double Sum = 0;
            if (this.IsOpen) //проверяем, что вклад открыт
            {
                if (!Capitalization && dateTime >= EndDate && IsActive) //если вклад с без капитализации
                {
                    Sum = ChargeInterestNonCapitalization(dateTime); //обращаемся к методу для расчета процентов по вкладу
                    IsActive = false; //деактивируем вклад
                }
                else if (Capitalization && dateTime >= CapitalizationDates.Peek() && IsActive) //если вклад с капитализацией
                { //и дата больше или равна следующей капитализации процентов
                    Sum = ChargeInterestCapitalization(dateTime);  //расчитываем проценты за период
                    IsActive = dateTime >= EndDate ? false : IsActive; //обновляем статус вклада
                }
            }
            return Sum;
        }

        /// <summary>
        /// Метод расчета суммы начисленных процентов по вкладу без капитализации
        /// </summary>
        /// <param name="dateTime">Текущая дата</param>
        /// <returns>Сумма начисленных процентов в double</returns>
        private double ChargeInterestNonCapitalization(DateTime dateTime)
        {
            double Sum = 0;
            if(dateTime >= EndDate && IsActive)
            {
                Sum = Math.Round(Balance / 100 * Percent, 2);
            }
            return Sum;
        }

        /// <summary>
        /// Метод расчета суммы начисленных процентов по вкладу с ежемесячной капитализацией
        /// </summary>
        /// <param name="dateTime">Текущая дата</param>
        /// <returns>Сумма начисленных процентов в double</returns>
        private double ChargeInterestCapitalization(DateTime dateTime)
        {
            double Sum = 0;
            if (dateTime >= CapitalizationDates.Peek() && IsActive)
            {
                double ActualPercent = Percent / 12;
                Sum = Math.Round(Balance / 100 * ActualPercent, 2);
                PreviousCapitalization = CapitalizationDates.Dequeue();
            }
            return Sum;
        }
        #endregion
    }
}
