using BankMiniSystem.Services;
using BankModel_Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Transaction.xaml
    /// </summary>
    public partial class AddTransaction : Window
    {
        private MainWindow main; //Экземпляр MainWindow

        internal ObservableCollection<Account> senderAccounts; //Коллекция счетов списания
        
        internal ObservableCollection<Account> recipientAccounts; //Коллекция счетов зачисления

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="window">Экземпляр MainWindow</param>
        public AddTransaction(MainWindow window)
        {
            InitializeComponent();
            main = window;
            senderAccounts = new ObservableCollection<Account>(); //инициализируем коллекции счетов
            recipientAccounts = new ObservableCollection<Account>();
            SenderBox.ItemsSource = main.bank.GetAllClients(); //привязываем всех клиентов к комбобоксу
            RecipientBox.ItemsSource = main.bank.GetAllClients(); //аналогично
            SenderAccountsTable.ItemsSource = senderAccounts; //привязываем счета списания к списку счетов списания
            RecipientAccountsTable.ItemsSource = recipientAccounts; //и к списку счетов зачисления
            GetAnnotation(); //подгружаем аннотацию к TextBlock

            Sum.PreviewTextInput += new TextCompositionEventHandler(textBox_PreviewTextInput); //проверяем вводимые символы в поле сумма
        }

        /// <summary>
        /// Аннотация к операции перевода средств
        /// </summary>
        private void GetAnnotation()
        {
            string AnnotationText = "Внимамание!\n " +
                "Убедитесь, что счета списания и зачисления открыты и соответствующие операции для них доступны (пополнение/снятие)." +
                " Отмена транзакции невозможна. В случае наличия ограничений транзакция сохраняется в истории, но не выполняется";
            Annotation.Text = AnnotationText;
        }

        /// <summary>
        /// Изменение списка счетов списания при изменении выбора клиента
        /// </summary>
        private void SenderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.bank.CreateAccountCol<Account>(senderAccounts, SenderBox.SelectedItem as Client);
        }

        /// <summary>
        /// Изменение списка счетов зачисления при изменении выбора клиента
        /// </summary>
        private void RecipientBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.bank.CreateAccountCol<Account>(recipientAccounts, RecipientBox.SelectedItem as Client);
        }

        /// <summary>
        /// Изменение выбранного счета списания при изменении выбранного счета в списке
        /// </summary>
        private void SenderAccountsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectSenderAccount.DataContext = SenderAccountsTable.SelectedItem as Account;
        }

        /// <summary>
        /// Изменение выбранного счета зачисления при изменении выбранного счета в списке
        /// </summary>
        private void RecipientAccountsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectRecipientAccount.DataContext = RecipientAccountsTable.SelectedItem as Account;
        }

        /// <summary>
        /// Кнопка Выполнить (выполняет перевод средств)
        /// </summary>
        private void Button_Click_Realise(object sender, RoutedEventArgs e)
        {
            if (SelectSenderAccount.DataContext != null && SelectRecipientAccount.DataContext != null)
            {
                main.bank.CreateTransaction(Transaction.TypeTransaction.CashlessTransfer, (SelectSenderAccount.DataContext as Account),
                    (SelectRecipientAccount.DataContext as Account), Convert.ToDouble(Sum.Text));
            }
            main.Refresh();
            Close();
        }

        /// <summary>
        /// Кнопка Отмена
        /// </summary>
        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Обработка вводимых символов
        /// </summary>
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ViewService.ValidateInputDecimalNumber(e.Text[0], Sum.Text); //проверяем вводимые символы, передавая в сервис
        }
    }
}