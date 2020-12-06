using BankMiniSystem.Services;
using BankModel_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для Cash.xaml
    /// </summary>
    public partial class Cash : Window
    {
        private MainWindow main; //Экземпляр MainWindow

        private Account selectedAccount; //Экземпляр счета

        private Transaction.TypeTransaction typeTransaction; //Enum Transaction

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="window">Экземпляр MainWindow</param>
        public Cash(MainWindow window)
        {
            InitializeComponent();
            main = window;
            selectedAccount = main.AccountTable.SelectedItem as Account; //инициализируем свойство selectedAccount
            SumBox.PreviewTextInput += new TextCompositionEventHandler(textBox_PreviewTextInput); //обрабатываем ввод символов в поле сумма
            AccountBox.DataContext = selectedAccount; //указываем ресурс для отображения свойств выбранного счета
            typeTransaction = Transaction.TypeTransaction.CashWithdrawal; //по умолчанию выбираем тип транзакции Снятие наличных
        }

        /// <summary>
        /// Обработка вводимых символов
        /// </summary>
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ViewService.ValidateInputDecimalNumber(e.Text[0], SumBox.Text); //проверяем вводимые символы, передавая в сервис
        }

        /// <summary>
        /// Кнопка Выполнить
        /// </summary>
        private void Realise_Click(object sender, RoutedEventArgs e)
        {
            //string result = ""; //создаем транзакцию в зависимости от выбранной операции
            if (typeTransaction == Transaction.TypeTransaction.CashWithdrawal && SumBox.Text != "")
            {  main.bank.CreateTransaction(typeTransaction, selectedAccount, null, Convert.ToDouble(SumBox.Text)); }
            else if (typeTransaction == Transaction.TypeTransaction.CashRefill && SumBox.Text != "")
            { main.bank.CreateTransaction(typeTransaction, null, selectedAccount, Convert.ToDouble(SumBox.Text)); }
            //MessageBox.Show(result, "Результат операции", MessageBoxButton.OK, MessageBoxImage.Information);
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

        /// <summary>
        /// Выбор в RadioButton Снятие наличных
        /// </summary>
        private void Withrawal_Click(object sender, RoutedEventArgs e)
        {
            typeTransaction = Transaction.TypeTransaction.CashWithdrawal;
        }

        /// <summary>
        /// Выбор в RadioButton Пополнение счета
        /// </summary>
        private void Refill_Click(object sender, RoutedEventArgs e)
        {
            typeTransaction = Transaction.TypeTransaction.CashRefill;
        }
    }
}
