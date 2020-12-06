using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    interface IAccount
    {
        /// <summary>
        /// Имя (тип счета)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Проценты
        /// </summary>
        double Percent { get; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        DateTime EndDate { get; }

        /// <summary>
        /// Дата последней капитализации капитализации
        /// </summary>
        DateTime PreviousCapitalization { get; set; }

        /// <summary>
        /// Все оставшиеся даты капитализации в очереди
        /// </summary>
        Queue<DateTime> CapitalizationDates { get; }

        /// <summary>
        /// Статус счета (true - активен, идет начисление процентов)
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Текстовое значение статуса счета
        /// </summary>
        string TextIsActive { get; }

        /// <summary>
        /// Баланс счета
        /// </summary>
        double Balance { get; set; }

        /// <summary>
        /// Начисление процентов по счету
        /// </summary>
        /// <param name="dateTime">Дата</param>
        /// <returns>double</returns>
        double CalculateInterest(DateTime dateTime);
    }
}
