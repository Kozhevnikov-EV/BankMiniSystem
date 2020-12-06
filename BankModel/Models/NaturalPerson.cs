using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Класс Физ. лицо (наследник класса Client)
    /// </summary>
    public class NaturalPerson : Client, IClient
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
        #endregion

        #region Конструкторы и методы
        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="Name">Имя</param>
        /// <param name="Surname">Фамилия</param>
        /// <param name="Birthday">Дата рождения</param>
        public NaturalPerson (string Name, string Surname, DateTime Birthday)
            : base ()
        {
            this.Name = Name;
            this.Surname = Surname;
            this.Birthday = Birthday;
        }

        /// <summary>
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        protected NaturalPerson()
        { }

        /// <summary>
        /// Метод редактирования свойств экземпляра класса NaturalPerson
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Surname"></param>
        /// <param name="Birthday"></param>
        public void Edit(string Name, string Surname, DateTime Birthday)
        {
            this.Name = Name;
            this.Surname = Surname;
            this.Birthday = Birthday;
        }
        #endregion
    }
}
