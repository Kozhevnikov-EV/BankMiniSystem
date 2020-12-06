using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankExtensions_Library
{
    /// <summary>
    /// Класс, содержащий методы расширения
    /// </summary>
    public static class Extention
    {
        /// <summary>
        /// Определяет, содержит ли последовательность указанные символы, используя компаратор проверки на равенство по умолчанию.
        /// </summary>
        /// <param name="Words">Последовательность - объект System.String</param>
        /// <param name="IllegalChars">Набор искомых символов</param>
        /// <returns>true Если исходная последовательность содержит хотя бы один элемент;
        /// в противном случае — false.</returns>
        public static bool ContainsChars(this string Words, char[] IllegalChars)
        {
            bool result = false;
            foreach(var ch in IllegalChars)
            {
                result = Words.Contains(ch) ? true : false;
                if(result) { break; }
            }
            return result;
        }

        /// <summary>
        /// Возвращает пустую строку, если массив символов пустой / возвращает строквое представление массива символов, если массив не пустой
        /// </summary>
        /// <param name="Chars">Массив символов</param>
        /// <returns>string</returns>
        public static string ToStringOrEmpty(this char[] Chars)
        {
            if (Chars == null || Chars.Length == 0) return String.Empty;
            else
            {
                StringBuilder SB = new StringBuilder();
                foreach (var ch in Chars)
                {
                    SB.Append(ch);
                }
                return SB.ToString();
            }
        }

        /// <summary>
        ///  Удаляет все вхождения набора знаков, заданного в виде массива, из текущего объекта System.String.
        /// </summary>
        /// <param name="Item">Объект System.String</param>
        /// <param name="Chars">Массив символов, которые необходимо удалить из объекта</param>
        /// <returns>string</returns>
        public static string TrimAllChars(this string Item, char[] Chars)
        {
            foreach(var ch in Chars)
            {
                Item = Item.Replace(ch.ToString(), "");
            }
            return Item;
        }
    }
}
