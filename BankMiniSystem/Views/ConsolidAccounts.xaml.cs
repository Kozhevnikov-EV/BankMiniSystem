using System;
using BankModel_Library;
using System.Collections.Generic;
using BankMiniSystem.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для ConsolidAccounts.xaml
    /// </summary>
    public partial class ConsolidAccounts : Window
    {
        private MainWindow main; //Экземпляр MainWindow

        internal ObservableCollection<Account> AccountsCol; //Коллекция счетов

        internal ConsolidAccounts(MainWindow window, Client client)
        {
            InitializeComponent();
            main = window;
            AccountsCol = new ObservableCollection<Account>(); //инициализируем коллекцию счетов
            ViewService.CreateOpenAccountCol(main.bank, AccountsCol, client); //наполняем коллекцию счетов
            AccountBox1.ItemsSource = AccountsCol; //привязываем коллекции счетов к комбобоксам
            AccountBox2.ItemsSource = AccountsCol;
        }

        /// <summary>
        /// Кнопка Объединить
        /// </summary>
        private void Realise_Click(object sender, RoutedEventArgs e)
        {
            //проверяем, что счета выбраны и они не одинаковые
            if(AccountBox1.SelectedItem != null && AccountBox2.SelectedItem != null &&
                AccountBox1.SelectedItem as Account != AccountBox2.SelectedItem as Account)
            {
                //Объединяем счета и выводим результат операции на экран
                string result = main.bank.ConsolidationAccounts(AccountBox1.SelectedItem as Account, AccountBox2.SelectedItem as Account);
                MessageBox.Show(result, "Объединение", MessageBoxButton.OK, MessageBoxImage.Information);
                main.Refresh();
                Close();
            }
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
