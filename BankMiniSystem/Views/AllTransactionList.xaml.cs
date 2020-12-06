using System;
using BankModel_Library;
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
    /// Логика взаимодействия для AllTransactionList.xaml
    /// </summary>
    public partial class AllTransactionList : Window
    {
        private ObservableCollection<Transaction> transactions; //Коллекция всех транзакций

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="transactions">Коллекция всех транзакций</param>
        internal AllTransactionList(ObservableCollection<Transaction> transactions)
        {
            InitializeComponent();
            this.transactions = transactions;
            TransactionsTable.ItemsSource = transactions;
        }
    }
}
