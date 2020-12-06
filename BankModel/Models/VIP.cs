using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Класс VIP клиент (наследник класса Client)
    /// </summary>
    public class VIP : Client, IClient
    {
        #region Поля и свойства
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Место работы
        /// </summary>
        public string WorkPlace { get; private set; }
        #endregion

        #region Конструкторы и методы
        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        /// <param name="WorkPlace">Место работы</param>
        public VIP(string Name, string Surname, DateTime Birthday, string WorkPlace)
            : base()
        {
            this.Name = Name;
            this.Surname = Surname;
            this.Birthday = Birthday;
            this.WorkPlace = WorkPlace;
        }

        /// <summary>
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        protected VIP()
        { }

        /// <summary>
        /// Метод редактирования свойств экземпляра класса VIP
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        /// <param name="WorkPlace">Место работы</param>
        public void Edit(string Name, string Surname, DateTime Birthday, string WorkPlace)
        {
            this.Name = Name;
            this.Surname = Surname;
            this.Birthday = Birthday;
            this.WorkPlace = WorkPlace;
        }
        #endregion
    }
}
