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
using System.Collections.ObjectModel;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для BankLogList.xaml
    /// </summary>
    public partial class BankLogList : Window
    {
        private ObservableCollection<ActivityInfo> LogList; //коллекция логов по изменению балансов счетов

        internal BankLogList(ObservableCollection<ActivityInfo> LogList)
        {
            InitializeComponent();
            this.LogList = LogList; //инициализируем коллекцию логов
            AccountLog.ItemsSource = LogList; //делаем привязку с помощью ItemSource
        }
    }
}
