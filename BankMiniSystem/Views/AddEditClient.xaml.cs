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
    /// Логика взаимодействия для AddEditClient.xaml
    /// </summary>
    public partial class AddEditClient : Window
    {
        private MainWindow main; //Экземпляр MainWindow

        private Client selectedClient; //Экземпляр клиента

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="window">Экземпляр MainWindow</param>
        /// <param name="client">Экземпляр клиента, которому создаем Вклад/Кредит</param>
        internal AddEditClient(MainWindow window, Client client)
        {
            InitializeComponent();
            main = window;
            selectedClient = client;
            BirthdayBox.SelectedDate = Bank.Today;
            if (selectedClient != null) { PreparingFieldsForEdit(); }
        }

        /// <summary>
        /// Заполняет поля окна редактирования клиентов в зависимости от типа клиента (NaturalPerson, VIP, Company)
        /// </summary>
        private void PreparingFieldsForEdit()
        {
            RadioButtonClient.Visibility = Visibility.Hidden;
            if (selectedClient.GetType() == typeof(NaturalPerson))
            {
                NaturalFieldVisibility();
                NaturalPerson client = selectedClient as NaturalPerson;
                NameBox.Text = client.Name;
                SurnameBox.Text = client.Surname;
                BirthdayBox.SelectedDate = client.Birthday;
            }
            else if (selectedClient.GetType() == typeof(VIP))
            {
                VIPFieldVisibiity();
                VIP client = selectedClient as VIP;
                NameBox.Text = client.Name;
                SurnameBox.Text = client.Surname;
                BirthdayBox.SelectedDate = client.Birthday;
                WorkPlaceBox.Text = client.WorkPlace;
            }
            else if (selectedClient.GetType() == typeof(Company))
            {
                CompanyFieldVisibility();
                Company client = selectedClient as Company;
                TypeOrgBox.Text = client.TypeOrg;
                NameOrgBox.Text = client.Name;
            }
        }

        /// <summary>
        /// Выбор типа клиента - Физ. лицо
        /// </summary>
        private void RadioButton_Natural(object sender, RoutedEventArgs e)
        {
            NaturalFieldVisibility();
        }

        /// <summary>
        /// Выбор типа клиента - VIP
        /// </summary>
        private void RadioButton_VIP(object sender, RoutedEventArgs e)
        {
            VIPFieldVisibiity();
        }

        /// <summary>
        /// Выбор типа клиента - Юр.лицо
        /// </summary>
        private void RadioButton_Company(object sender, RoutedEventArgs e)
        {
            CompanyFieldVisibility();
        }

        #region Методы настройки видимости полей
        private void NaturalFieldVisibility()
        {
            NameBlock.Visibility = Visibility.Visible;
            NameBox.Visibility = Visibility.Visible;
            SurnameBlock.Visibility = Visibility.Visible;
            SurnameBox.Visibility = Visibility.Visible;
            BirthdayBlock.Visibility = Visibility.Visible;
            BirthdayBox.Visibility = Visibility.Visible;
            WorkPlaceBlock.Visibility = Visibility.Collapsed;
            WorkPlaceBox.Visibility = Visibility.Collapsed;
            TypeOrgBlock.Visibility = Visibility.Collapsed;
            TypeOrgBox.Visibility = Visibility.Collapsed;
            NameOrgBlock.Visibility = Visibility.Collapsed;
            NameOrgBox.Visibility = Visibility.Collapsed;
        }

        private void VIPFieldVisibiity()
        {
            NameBlock.Visibility = Visibility.Visible;
            NameBox.Visibility = Visibility.Visible;
            SurnameBlock.Visibility = Visibility.Visible;
            SurnameBox.Visibility = Visibility.Visible;
            BirthdayBlock.Visibility = Visibility.Visible;
            BirthdayBox.Visibility = Visibility.Visible;
            WorkPlaceBlock.Visibility = Visibility.Visible;
            WorkPlaceBox.Visibility = Visibility.Visible;
            TypeOrgBlock.Visibility = Visibility.Collapsed;
            TypeOrgBox.Visibility = Visibility.Collapsed;
            NameOrgBlock.Visibility = Visibility.Collapsed;
            NameOrgBox.Visibility = Visibility.Collapsed;
        }

        private void CompanyFieldVisibility()
        {
            NameBlock.Visibility = Visibility.Collapsed;
            NameBox.Visibility = Visibility.Collapsed;
            SurnameBlock.Visibility = Visibility.Collapsed;
            SurnameBox.Visibility = Visibility.Collapsed;
            BirthdayBlock.Visibility = Visibility.Collapsed;
            BirthdayBox.Visibility = Visibility.Collapsed;
            WorkPlaceBlock.Visibility = Visibility.Collapsed;
            WorkPlaceBox.Visibility = Visibility.Collapsed;
            TypeOrgBlock.Visibility = Visibility.Visible;
            TypeOrgBox.Visibility = Visibility.Visible;
            NameOrgBlock.Visibility = Visibility.Visible;
            NameOrgBox.Visibility = Visibility.Visible;
        }
        #endregion

        /// <summary>
        /// Кнопка сохранить
        /// </summary>
        private void AddEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient == null) //если создаем нового клиента
            {
                if (NaturalCheck.IsChecked == true)
                { main.bank.AddNatural(NameBox.Text, SurnameBox.Text, BirthdayBox.SelectedDate.Value); }
                else if (VIPcheck.IsChecked == true)
                { main.bank.AddVIP(NameBox.Text, SurnameBox.Text, BirthdayBox.SelectedDate.Value, WorkPlaceBox.Text); }
                else if (CompanyCheck.IsChecked == true) { main.bank.AddCompany(TypeOrgBox.Text, NameBox.Text); }
            }
            else //или правим существующего
            {
                main.bank.EditClient<Client>(
                    selectedClient, NameBox.Text, SurnameBox.Text, BirthdayBox.SelectedDate.Value,
                    WorkPlaceBox.Text, TypeOrgBox.Text, NameOrgBox.Text);
            }
            main.Refresh();
            Close();
        }

        /// <summary>
        /// Кнопка отмена
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
