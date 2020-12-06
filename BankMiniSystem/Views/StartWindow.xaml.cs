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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void Button_NewBank(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.MenuItem_NewBank(sender, e);
            window.Show();
            Close();
        }

        /// <summary>
        /// Кнопка Загрузить
        /// </summary>
        private void Button_Load(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow(); //создаем экземпляр нового окна
            window.MenuItem_Open(sender, e); //в нем загрузаемся с Base.json
            window.Show(); //выводим на экран
            Close();
        }

        /// <summary>
        /// Кнопка Демонстрация
        /// </summary>
        private void Button_Random(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.MenuItem_Random(sender, e);
            window.Show();
            Close();
        }

        /// <summary>
        /// Кнопка Выход
        /// </summary>
        private void Button_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
