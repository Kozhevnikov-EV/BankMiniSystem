using BankMiniSystem.Services;
using BankMiniSystem.Views;
using BankModel_Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace BankMiniSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Поля и свойства
        /// <summary>
        /// Экземпляр банка
        /// </summary>
        internal Bank bank { get; set; }

        /// <summary>
        /// Коллекция физ. лиц для отображения в главном окне в DataGrid
        /// </summary>
        internal ObservableCollection<NaturalPerson> naturals { get; set; }

        /// <summary>
        /// Коллекция ВИП клиентов для отображения в главном окне в DataGrid
        /// </summary>
        internal ObservableCollection<VIP> VIPs { get; set; }

        /// <summary>
        /// Коллекция Юр. лиц для отображения в главном окне в DataGrid
        /// </summary>
        internal ObservableCollection<Company> companies { get; set; }

        /// <summary>
        /// Коллекция счетов клиента для отображения в DataGrid
        /// </summary>
        internal ObservableCollection<Account> accounts { get; set; }

        /// <summary>
        /// Коллекция сообщений счетов об изменении балансов
        /// </summary>
        internal ObservableCollection<ActivityInfo> activities { get; set; }

        /// <summary>
        /// Текущий выбранный клиент в DataGrid
        /// </summary>
        internal Client client { get; set; }
        #endregion

        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();
            bank = new Bank();
            bank.BankException += ExceptionHandler.Handler;
            naturals = new ObservableCollection<NaturalPerson>(); //инициализируем коллекции клиентов
            VIPs = new ObservableCollection<VIP>();
            companies = new ObservableCollection<Company>();
            accounts = new ObservableCollection<Account>(); //и счетов
            activities = new ObservableCollection<ActivityInfo>();

            NaturalsTable.ItemsSource = naturals; //осуществляем привязку коллекций клиентов к DataGrid-ам
            VIPsTable.ItemsSource = VIPs;
            CompaniesTable.ItemsSource = companies;
            AccountTable.ItemsSource = accounts; //и привязку счетов
            LogPanel.ItemsSource = activities;
            this.Closing += MainWindow_Closed; //Метод, вызываемый при закрытии программы пользователем
        }
        #endregion

        #region Вспомогательные методы
        private void MainWindow_Closed(object sender, CancelEventArgs e)
        {
            ///Создаем экземпляр MessageBox
            var UserAnswer = MessageBox.Show("Сохранить изменения?",
                                $"Выход из программы {this.Title}",
                                MessageBoxButton.YesNoCancel,
                                MessageBoxImage.Question);
            if (UserAnswer == MessageBoxResult.Yes) //если пользователь подтвердил сохранение
            {
                bank.SaveToJson(@"base.json"); //сохраняем
                e.Cancel = false; //не отменяем закрытие программы
            }
            else if (UserAnswer == MessageBoxResult.No) //если пользователь подтвердил закрытие программы без сохранения
            {
                e.Cancel = false; //закрываем программу
            }
            else //если пользователь отменил закрытие программы
            {
                e.Cancel = true; //отменяем закрытие программы
            }
        }

        /// <summary>
        /// Обновление данных, отображаемых в MainWindow
        /// </summary>
        public void Refresh()
        {
            ViewService.CreateClientCol(bank, naturals);
            ViewService.CreateClientCol(bank, VIPs);
            ViewService.CreateClientCol(bank, companies);
            accounts.Clear();
            ViewService.CreateAccountCol(bank, accounts, client);
            ViewService.GetLogCollection(bank, activities);
            TodayBox.Text = $"Сегодня {Bank.Today.ToShortDateString()}";
            Title = bank.Name.ToString();
        }

        /// <summary>
        /// Метод вывода информации о счетах клиента при его выборе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender == NaturalsTable) { client = NaturalsTable.SelectedItem as Client; }
            else if(sender == VIPsTable) { client = VIPsTable.SelectedItem as Client; }
            else if(sender == CompaniesTable) { client = CompaniesTable.SelectedItem as Client; }
            ViewService.CreateAccountCol(bank, accounts, client);
        }

        /// <summary>
        /// Обновляет свойство client в MainWindow (текущий выбранный клиент)
        /// </summary>
        private void SelectClient()
        {
            client = null;
            if (NaturalTab.IsSelected && NaturalsTable.SelectedItem is Client) { client = NaturalsTable.SelectedItem as Client; }
            else if (VIPTab.IsSelected && VIPsTable.SelectedItem is Client) { client = VIPsTable.SelectedItem as Client; }
            else if (CompanyTab.IsSelected && CompaniesTable.SelectedItem is Client) { client = CompaniesTable.SelectedItem as Client; }
        }
        #endregion

        #region Главное меню программы
        /// <summary>
        /// Создание нового экземпляра Bank
        /// </summary>
        internal void MenuItem_NewBank(object sender, RoutedEventArgs e)
        {
            bank = new Bank("Новый банк", 12);
            bank.BankException += ExceptionHandler.Handler; //подписываем новый экзепляр банка на событие BankException
            Refresh();
        }

        /// <summary>
        /// Открытие экземпляра Bank из Base.json + подписка на событие BankException
        /// </summary>
        internal void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            bank = new Bank();
            bank.BankException += ExceptionHandler.Handler; //подписываем новый экзепляр банка на событие BankException
            //чтобы ловить исключения при загрузке с json
            bank = bank.LoadFromJson(@"base.json"); //загружаемся с json
            bank.BankException += ExceptionHandler.Handler; //снова подписываем экзепляр банка на событие BankException, т.к.
            //после загрузки с json bank имеет ссылку на новый экземпляр Bank(загруженный с json)
            Refresh();
        }

        /// <summary>
        /// Сохранение текущего bank в Base.Json
        /// </summary>
        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            bank.SaveToJson(@"base.json");
        }

        /// <summary>
        /// Создание демонстрационного экземпляра Bank
        /// </summary>
        internal void MenuItem_Random(object sender, RoutedEventArgs e)
        {
            bank = new Bank();
            bank = RandomBank.CreateRandomBank();
            Refresh();
        }

        /// <summary>
        /// Изменение наименование банка и базовой процентной ставки
        /// </summary>
        private void MenuItem_Click_Settings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this);
            settingsWindow.Owner = this;
            settingsWindow.Show();
        }
        #endregion

        #region Основные кнопки управления и контекстное меню
        /// <summary>
        /// Кнопка Новый клиент
        /// </summary>
        private void Button_Click_AddClient(object sender, RoutedEventArgs e)
        {
            AddEditClient addClient = new AddEditClient(this, null);
            addClient.Owner = this;
            addClient.Show();
        }

        /// <summary>
        /// Кнопка Редактировать клиента
        /// </summary>
        private void MenuItem_Click_EditClient(object sender, RoutedEventArgs e)
        {
            SelectClient();
            if(client != null) //если клиент был выбран, то
            {
                AddEditClient addClient = new AddEditClient(this, client);
                addClient.Owner = this;
                addClient.Show();
            }
        }

        /// <summary>
        /// Кнопка Новый счет
        /// </summary>
        private void MenuItem_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            SelectClient();
            if (client != null) //если клиент был выбран, то
            {
                bank.AddAccount(client.Id, 0);
                Refresh();
            }
        }

        /// <summary>
        /// Контекстное меню Закрыть счет
        /// </summary>
        private void MenuItem_Click_CloseAccount(object sender, RoutedEventArgs e)
        {
            if(AccountTable.SelectedItem != null)
            {
                bank.CloseAccount((AccountTable.SelectedItem as Account).Id);
                Refresh();
            }
        }

        /// <summary>
        /// Кнопка Перевод средств
        /// </summary>
        private void Button_Click_CreateTransaction(object sender, RoutedEventArgs e)
        {
            AddTransaction transactionWin = new AddTransaction(this);
            transactionWin.Owner = this;
            transactionWin.Show();
        }

        /// <summary>
        /// Кнопка и контекстное меню Операции с наличными
        /// </summary>

        private void MenuItem_Click_Cash(object sender, RoutedEventArgs e)
        {
            if (AccountTable.SelectedItem != null)
            {
                Cash cash = new Cash(this);
                cash.Owner = this;
                cash.Show();
            }
            else { MessageBox.Show("Счет не выбран", "Операция невозможна!", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        /// <summary>
        /// Кнопка Все операции(транзакции)
        /// </summary>
        private void Button_Click_AllTransaction(object sender, RoutedEventArgs e)
        {
            if(bank != null)
            {
                AllTransactionList allTransactionList = new AllTransactionList(bank.Transactions);
                allTransactionList.Owner = this;
                allTransactionList.Show();
            }
        }

        /// <summary>
        /// Контекстное меню Операции по счету
        /// </summary>
        private void MenuItem_Click_TransactionsOfAccount(object sender, RoutedEventArgs e)
        {
            if(AccountTable.SelectedItem != null)
            {
                AllTransactionList transactionList = new AllTransactionList(
                    ViewService.GetTransactionsOfAccount(bank, AccountTable.SelectedItem as Account));
                transactionList.Owner = this;
                transactionList.Show();
            }
        }

        /// <summary>
        /// Кнопка Новый вклад/кредит
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_AddDepositCredit(object sender, RoutedEventArgs e)
        {
            SelectClient();
            if (client != null) //если клиент был выбран, то
            {
                AddDepositCredit addDepositCredit = new AddDepositCredit(this, client);
                addDepositCredit.Owner = this;
                addDepositCredit.Show();
            }
        }

        /// <summary>
        /// Кнопка Добавить месяц
        /// </summary>
        private void Button_Click_PlusMonth(object sender, RoutedEventArgs e)
        {
            bank?.TimeMachine();
            Refresh();
        }

        /// <summary>
        /// Кнопка Справка
        /// </summary>
        private void Button_Click_Rules(object sender, RoutedEventArgs e)
        {
            Rules rules = new Rules();
            rules.Owner = this;
            rules.Show();
        }

        /// <summary>
        /// Двойное нажатие по окошку с логами
        /// </summary>
        private void LogPanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BankLogList logList = new BankLogList(bank.LogList);
            logList.Owner = this;
            logList.Show();
        }

        /// <summary>
        /// Кнопка Объединить (объединяет два счета)
        /// </summary>
        private void Button_Click_Consolidation(object sender, RoutedEventArgs e)
        {
            ConsolidAccounts consolidAccounts = new ConsolidAccounts(this, client);
            consolidAccounts.Owner = this;
            consolidAccounts.Show();
        }

        /// <summary>
        /// Кнопка График балансов
        /// </summary>
        private void Button_Click_BalanceLog(object sender, RoutedEventArgs e)
        {
            if (AccountTable.SelectedItem as Account != null)
            {
                BalanceHistogram balanceHistogram = new BalanceHistogram(AccountTable.SelectedItem as Account);
                balanceHistogram.Owner = this;
                balanceHistogram.Show();
            }
        }
        #endregion
    }
}
