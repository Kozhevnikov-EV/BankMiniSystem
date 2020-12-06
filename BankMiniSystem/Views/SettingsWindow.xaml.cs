using BankMiniSystem.Services;
using BankModel_Library;
using System;
using InvalidCharException_Library;
using BankExtensions_Library;
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
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow main; //Экземпляр MainWindow

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="window">Экземпляр MainWindow</param>
        public SettingsWindow(MainWindow window)
        {
            InitializeComponent();
            main = window;
            NameBox.Text = main.bank.Name;
            PercentBox.Text = Bank.BaseRate.ToString();
            char[] WE = new char[0];
            CharBox.Text = InvalidCharException.CharCollection.ToStringOrEmpty();
            PercentBox.PreviewTextInput += new TextCompositionEventHandler(textBox_PreviewTextInput); //обработка вводимых символов в базовую ставку
        }

        /// <summary>
        /// Обработка вводимых символов
        /// </summary>
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ViewService.ValidateInputDecimalNumber(e.Text[0], PercentBox.Text); //проверяем вводимые символы, передавая в сервис
        }

        /// <summary>
        /// Кнопка Сохранить
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(NameBox.Text != "" && PercentBox.Text != "")
            {
                main.bank.BankSettings(NameBox.Text, Convert.ToDouble(PercentBox.Text), CharBox.Text.ToArray());
            }
            MessageBox.Show($"Наименование банка: {main.bank.Name}\nБазовая ставка: {Bank.BaseRate} %",
                "Выполнено!", MessageBoxButton.OK, MessageBoxImage.Information);
            main.Refresh();
            Close();
        }

        /// <summary>
        /// Кнопка Отмена
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
