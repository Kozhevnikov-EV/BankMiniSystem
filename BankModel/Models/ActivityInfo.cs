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
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; }

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
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        protected ActivityInfo(DateTime Date, string Message)
            : this(Message)
        {
            this.Date = Date;
        }
    }
}
