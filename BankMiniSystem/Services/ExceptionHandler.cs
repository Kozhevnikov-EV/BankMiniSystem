using System;
using InvalidCharException_Library;
using Newtonsoft.Json;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMiniSystem
{
    /// <summary>
    /// Статический класс - обработчик исключений
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// Обрабатывает входящее исключение и выводит на экран MessageBox с описанием ошибки
        /// </summary>
        /// <param name="Ex"></param>
        public static void Handler(Exception Ex)
        {
            //если исключение - файл не найден (System.IO)
            if (Ex is FileNotFoundException) { MessageBox.Show(Ex.Message, "Файл не найден!", MessageBoxButton.OK, MessageBoxImage.Error); }
            //если исключение - ошибка десериализации (Newtonsoft.Json)
            else if (Ex is JsonException) { MessageBox.Show(Ex.Message, "Файл base.json поврежден!", MessageBoxButton.OK, MessageBoxImage.Error); }
            //если исключение - наличие запрещенных символов (InvalidCharExceptionLibrary)
            else if (Ex is InvalidCharException) { MessageBox.Show(Ex.Message, "Недопустимое наименование", MessageBoxButton.OK, MessageBoxImage.Error); }
            //все прочие исключения
            else { MessageBox.Show(Ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
