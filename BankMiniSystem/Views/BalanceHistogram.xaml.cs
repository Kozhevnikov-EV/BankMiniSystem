using System;
using BankModel_Library;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для BalanceeHistogram.xaml
    /// </summary>
    public partial class BalanceHistogram : Window
    {
        internal BalanceHistogram(List<BalanceLog> logs)
        {
            InitializeComponent();
            if(logs.Count != 0) Graph.Data = GetBalanceInPercent(logs); //вызываем метод, который создает коллекцию BalanceLog для отображения
        }

        /// <summary>
        /// Подготавливает коллекцию для отображения в Histogram
        /// Пересчитывает значение Balance (подробнее в комментариях внутри метода)
        /// </summary>
        /// <param name="log">Исходная коллекция</param>
        /// <returns></returns>
        private List<BalanceLog> GetBalanceInPercent(List<BalanceLog> log)
        {
            List<BalanceLog> ColInPercent = new List<BalanceLog>(log.Count); //новая коллекция, в которую добавим значения BalanceLog для отображения
            double Max = log.Max().Balance; //Максимальное значение баланса исходной коллекции
            foreach (var e in log) //наполняем коллекцию для отображения в цикле
            {
                BalanceLog LogForHistogram = (BalanceLog)e.Clone(); //копируем элемент исходной коллекции
                //В копии меняем значение баланса = значение исходного баланса выражаем в виде доли от максимального значения баланса
                //вычисленную долю умножаем на (ширина окна за вычетом 50).
                //Из ширины окна вычитаем 50 для предотвращения выхода за границы окна максимального значения (компенсация отступа Rectangle
                //и StackPanel
                LogForHistogram.Balance = LogForHistogram.Balance / Max * (this.Width - 50);
                ColInPercent.Add(LogForHistogram); //добавляем получившийся элемент в коллекцию

            }
            return ColInPercent;
        }
    }
}
