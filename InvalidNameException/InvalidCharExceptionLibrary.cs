using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvalidCharException_Library
{
    public class InvalidCharException : Exception
    {
        /// <summary>
        /// Статичная коллекция недопустимых символов
        /// </summary>
        public static char[] CharCollection { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public InvalidCharException()
            : base()
        {
            Message = $"Недопустимо использовать символы: {GetIllegalChars()}";
        }

        /// <summary>
        /// Возвращает коллекцию недопустимых символов в string
        /// </summary>
        /// <returns>string</returns>
        private string GetIllegalChars()
        {
            StringBuilder SB = new StringBuilder();
            foreach (var e in CharCollection)
            {
                SB.Append($"{e} ");
            }
            return SB.ToString();
        }

        //public static void SetIllegalChars(char[] IllegalChars)
        //{
        //    InvalidCharException_Library.InvalidCharException.CharCollection = IllegalChars;
        //}

        //public static void SetIllegalChars(string IllegalChars)
        //{
        //    InvalidCharException_Library.InvalidCharException.CharCollection = IllegalChars.ToCharArray();
        //}


    }
}
