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
    /// Класс Company (наследник класса Client)
    /// </summary>
    [Table("Companies")]
    public class Company : Client, IClient
    {
        #region Поля и свойства
        /// <summary>
        /// Тип юрлица (организационная форма) ООО/ОАО/АО итд
        /// </summary>
        public string TypeOrg { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region Конструкторы и методы
        /// <summary>
        /// Конструктор для EF
        /// </summary>
        private Company() { }

        /// <summary>
        /// Базовый конструктор (без присвоения Id)
        /// </summary>
        /// <param name="TypeOrg">Тип организации</param>
        /// <param name="Name">Наименование</param>
        public Company(string TypeOrg, string Name)
            : base()
        {
            this.TypeOrg = TypeOrg;
            this.Name = Name;
        }

        /// <summary>
        /// Метод редактирования свойств экземпляра класса Company
        /// </summary>
        /// <param name="TypeOrg">Тип юрлица</param>
        /// <param name="Name">Наименование организации</param>
        public void Edit(string TypeOrg, string Name)
        {
            this.TypeOrg = TypeOrg;
            this.Name = Name;
        }
        #endregion
    }
}
