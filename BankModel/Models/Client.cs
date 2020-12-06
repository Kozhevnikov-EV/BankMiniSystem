using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankModel_Library
{
    /// <summary>
    /// Супер-класс Клиент
    /// </summary>
    public abstract class Client 
    {
        #region Статичное поле, конструктор и методы для работы с Id
        /// <summary>
        /// Статическое поле для staticId
        /// </summary>
        [JsonIgnore]
        private static int staticId;

        /// <summary>
        /// Статичный конструктор для инициализации staticId
        /// </summary>
        static Client()
        {
            staticId = 0;
        }

        /// <summary>
        /// Статический метод для получения следущего Id
        /// </summary>
        /// <returns>Id</returns>
        private static int NextId()
        {
            staticId++;
            return staticId;
        }

        /// <summary>
        /// Метод обнуления значения staticId
        /// </summary>
        public static void ResetStaticId()
        {
            staticId = 0;
        }
        #endregion

        #region Поля и свойства
        /// <summary>
        /// Личный номер
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Поле кредитный рейтинг
        /// </summary>
        private int creditRating;

        /// <summary>
        /// Свойство кредитный рейтинг
        /// </summary>
        public int CreditRating
        {
            get { return creditRating; }
            set
            { //кредитный рейтинг может устанавливаться от -3 до 3
                if (value < -3) { creditRating = -3; }
                else if (value > 3) { creditRating = 3; }
                else { creditRating = value; }
            }
        }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Базовый коструктор
        /// </summary>
        public Client()
        {
            Id = NextId();
            CreditRating = 0;
        }

        /// <summary>
        /// Конструктор для Json
        /// </summary>
        [JsonConstructor]
        protected Client(int Id, int CreditRating)
        {
            this.Id = Id;
            this.CreditRating = CreditRating;
        }
        #endregion

        /// <summary>
        /// Метод изменения кредитного рейтинга экземпляра Client
        /// </summary>
        /// <param name="Up">true - повышает на 1, false - уменьшает на 1</param>
        public void ChangeCreditRating(bool Up)
        {
            CreditRating = Up ? CreditRating + 1 : CreditRating - 1;
        }
    }
}
