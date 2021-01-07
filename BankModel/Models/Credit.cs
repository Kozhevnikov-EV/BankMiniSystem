using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Класс Кредит (наследник Account)
    /// </summary>
    public class Credit : Account, IAccount
    {
        #region Поля и свойства + Событие
        /// <summary>
        /// Событие закрытия кредита
        /// </summary>
        public event Action<bool> CreditFinished;

        /// <summary>
        /// Имя(тип) (переопределен)
        /// </summary>
        public override string Name => "Кредит";

        /// <summary>
        /// Установленный процент по кредиту (годовой)
        /// </summary>
        public double Percent { get; }

        /// <summary>
        /// Сумма, полученная клиентом по кредиту (стартовая сумма основного долга)
        /// </summary>
        public double StartDebt { get; }

        /// <summary>
        /// Текущий размер основного долга
        /// </summary>
        public double CurentDebt { get; private set; }

        /// <summary>
        /// Ежемесячный платеж для погашения основного долга (без учета начисленных процентов)
        /// </summary>
        public double MonthlyPayment { get; }

        /// <summary>
        /// Дата окончания срока действия
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// Дата предыдущего списания процентов
        /// </summary>
        public DateTime PreviousCapitalization { get; set; }

        /// <summary>
        /// Даты списания процентов по вкладу
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
                isActive = value;
                if (!value) { CreditFinished?.Invoke(WithoutNegativeBalance); } //вызываем событие закрытия кредита
            }
        }

        /// <summary>
        /// Текстовый статус вклада (Да - проценты начисляются)
        /// </summary>
        public string TextIsActive { get { return IsActive ? "Да" : "Нет"; } }

        /// <summary>
        /// Текущий баланс счета (переопределенное свойство)
        /// </summary>
        public override double Balance
        {
            get
            { return base.balance; }
            set
            {
                if (value == 0 && !IsActive) { base.balance = value; base.IsOpen = false; } //если кредит закрыт и баланс 0 - закрываем его
                else //только у кредитного счета баланс может быть отрицательным
                { 
                    base.balance = Math.Round(value, 2);
                    if (balance < 0) { WithoutNegativeBalance = false; } //если после списания баланс отрицательный - регистрируем просрочку
                }
                if (IsActive)
                { IsActive = Bank.Today >= EndDate ? false : IsActive; }//проверяем срок действия и обновляем статус
            }
        }

        /// <summary>
        /// Свойство, характеризующее наличие просрочек платежей (false - баланс хотя бы единожды был отрицательным)
        /// </summary>
        public bool WithoutNegativeBalance { get; private set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Основной конструктор экземпляра Credit (Без присвоения Id)
        /// </summary>
        /// <param name="clientId">Id владельца счета</param>
        /// <param name="startBalance">Стартовая сумма на счете</param>
        /// <param name="percent">Процент по кредиту</param>
        /// <param name="endDate">Дата окончания кредита</param>
        /// <param name="startDebt">Сумма кредита (сумма задолженности)</param>
        public Credit(int clientId, double startBalance, double percent,  DateTime endDate, double startDebt)
            : base(clientId, startBalance)
        {
            //Логика присваивания процента
            this.Percent = (percent <= 0 || percent > 100) ? this.Percent = Bank.BaseRate : Math.Round(percent, 2);
            //Далее присваиваем Свойства экземпляра класса из параметров, переданных в конструктор
            EndDate = endDate;
            IsActive = true;
            IsRefill = true;
            IsWithdrawal = true;
            WithoutNegativeBalance = true;
            PreviousCapitalization = OpenDate;
            StartDebt = Math.Round(startDebt, 2);
            CurentDebt = StartDebt;
            //Создаем коллекцию дат капитализации процента
            CapitalizationDates = new Queue<DateTime>();
            DateTime NextCapitalization = OpenDate;
            do
            {
                NextCapitalization = NextCapitalization.AddMonths(1);
                CapitalizationDates.Enqueue(NextCapitalization);
            } while (NextCapitalization < EndDate);
            //расчитываем размер ежемесячного погашения основного долга
            MonthlyPayment = StartDebt / CapitalizationDates.Count;
        }

        /// <summary>
        /// Конструктор для SQL (с прямым присовоением Id из БД)
        /// </summary>
        public Credit(int Id, int ClientId, bool IsOpen, bool IsRefill, bool IsWithdrawal, DateTime OpenDate, double Balance, double Percent,
                DateTime EndDate, DateTime PreviousCapitalization, bool IsActive, double StartDebt,
                double CurentDebt, double MonthlyPayment, bool WithoutNegativeBalance)
                : base(Id, ClientId, IsOpen, IsRefill, IsWithdrawal, OpenDate, Balance)
        {
            this.Percent = Percent;
            this.EndDate = EndDate;
            this.PreviousCapitalization = PreviousCapitalization;
            this.CapitalizationDates = CapitalizationDates;
            this.IsActive = IsActive;
            this.StartDebt = StartDebt;
            this.CurentDebt = CurentDebt;
            this.MonthlyPayment = MonthlyPayment;
            this.WithoutNegativeBalance = WithoutNegativeBalance;
            CapitalizationDates = new Queue<DateTime>();
            DateTime NextCapitalization = PreviousCapitalization;
            do
            {
                NextCapitalization = NextCapitalization.AddMonths(1);
                CapitalizationDates.Enqueue(NextCapitalization);
            } while (NextCapitalization < EndDate);
        }
        #endregion

        #region Методы

        /// <summary>
        /// Метод расчета ежемесячного платежа (сумма ежемесячное погашение основного долга + проценты на остаток)
        /// </summary>
        /// <param name="dateTime">Текущая дата</param>
        /// <returns>double (ежемесячный платеж)</returns>
        public double CalculateInterest(DateTime dateTime)
        {
            double Sum = 0;
            if (this.IsOpen && IsActive && dateTime >= CapitalizationDates.Peek())
            {
                double ActualPercent = Percent / 12; //процент за месяц
                Sum = CurentDebt / 100 * ActualPercent + MonthlyPayment; //ежемесячный платеж (погашение основного долга + проценты)
                CurentDebt -= MonthlyPayment; //уменьшаем остаток основного долга
                PreviousCapitalization = CapitalizationDates.Dequeue(); //обновляем дату последнего списания процентов и удаляем ее из очереди
            }
            return Sum;
        }
        #endregion
    }
}
