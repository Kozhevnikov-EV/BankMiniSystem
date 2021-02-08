using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvalidCharException_Library;

namespace BankModel_Library
{
    /// <summary>
    /// Класс, хранящий некоторые свойства банка, в т.ч. статичные для сохранения и считывания с БД через BankSettingsContext
    /// </summary>
    public class BankSettings
    {
        public int Id { get; set; }

        /// <summary>
        /// Отображает текущую дату
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// Базовая ставка - поле
        /// </summary>
        private double baseRate;

        /// <summary>
        /// Базовая ставка % (применяется для расчета % по кредитам и вкладам)
        /// </summary>
        public double BaseRate
        {
            get { return baseRate; }
            set { baseRate = (value > 0 && value <= 100) ? value : 10; }
        }

        private char[] illegalCharsInName;

        /// <summary>
        /// Недопустимые символы в Name банка
        /// </summary>
        public char[] IllegalCharsInName 
        { get { return illegalCharsInName; }
            set
            {
                illegalCharsInName = value;
                InvalidCharException.CharCollection = illegalCharsInName;
            }
        }

        /// <summary>
        /// Наименование банка
        /// </summary>
        public string Name { get; set; }

        public BankSettings() 
        {
            Id = 1;
        }
    }
}
