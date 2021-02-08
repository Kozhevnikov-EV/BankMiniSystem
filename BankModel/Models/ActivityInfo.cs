using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Класс, содержащий информацию об изменении чего либо (дата + сообщение об изменении)
    /// </summary>
    public class ActivityInfo
    {
        public int Id { get; private set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Информационное сообщение
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// Основной контруктор
        /// </summary>
        /// <param name="Message">Информационное сообщение</param>
        public ActivityInfo(string Message)
        {
            Date = Bank.Today;
            this.Message = Message;
        }

        /// <summary>
        /// Конструктор c произвольным присвоением даты (используется в классах-наследниках)
        /// </summary>
        protected ActivityInfo(DateTime Date, string Message)
            : this(Message)
        {
            this.Date = Date;
        }

        /// <summary>
        /// Конструктор для EF
        /// </summary>
        protected ActivityInfo() { }
    }
}
