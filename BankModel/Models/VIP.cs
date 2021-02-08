using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Класс VIP клиент (наследник класса Client)
    /// </summary>
    [Table("VIPs")]
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
        /// Конструктор для EF
        /// </summary>
        private VIP() { }

        /// <summary>
        /// Базовый конструктор (без присвоения Id)
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
