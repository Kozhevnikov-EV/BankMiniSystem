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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankMiniSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для AddDepositCredit.xaml
    /// </summary>
    public partial class AddDepositCredit : Window
    {
        private MainWindow main; //экземпляр MainWindow

        private Client client; //Выбранный в MainWindow экземпляр клиента

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="window">Экземпляр MainWindow</param>
        /// <param name="selectedClient">Экземпляр клиента, которому создаем Вклад/Кредит</param>
        internal AddDepositCredit(MainWindow window, Client selectedClient)
        {
            InitializeComponent();
            main = window;
            client = selectedClient;
            ClientBox.DataContext = selectedClient;
            SumBox.PreviewTextInput += new TextCompositionEventHandler(textBox_PreviewTextInput); //проверяем вводимые символы в поле сумма
        }

        /// <summary>
        /// Обработка вводимых символов
        /// </summary>
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ViewService.ValidateInputDecimalNumber(e.Text[0], SumBox.Text); //проверяем вводимые символы, передавая в сервис
        }

        /// <summary>
        /// Кнопка добавить(выполнить)
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonDeposit.IsChecked == true) //если выбрано создание вклада
            {
                bool capitalization = CapitalizationTrue.IsChecked == true ? true : false; //переменная - наличие капитализации

                if (SumBox.Text != "" && Convert.ToDouble(SumBox.Text) > 0) //создаем вклад
                {
                    main.bank.AddDeposit(
                        client, Convert.ToDouble(SumBox.Text), Convert.ToDouble(PercentBox.Text), capitalization, Convert.ToInt32(PeriodBox.Text));
                }
            }
            else if (RadioButtonCredit.IsChecked == true) //если выбран кредит
            {
                if (SumBox.Text != "" && Convert.ToDouble(SumBox.Text) > 0) //создаем кредит
                {
                    main.bank.AddCredit(
                        client, Convert.ToDouble(SumBox.Text), Convert.ToDouble(PercentBox.Text), Convert.ToDouble(SumBox.Text), 
                        Convert.ToInt32(PeriodBox.Text));
                }
            }
            main.Refresh();
            Close();
        }

        /// <summary>
        /// Кнопка отмены
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Метод обработки события изменения значения слайдера
        /// </summary>
        private void PeriodSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PeriodBox.Text = PeriodSlider.Value.ToString();
        }

        /// <summary>
        /// Событие выбора Вклада
        /// </summary>
        private void Deposit_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonCapitalization.Visibility = Visibility.Visible;
            Title = "Новый вклад";
            PercentBox.Text = main.bank.GetActualDepositPercent(client).ToString();
        }

        /// <summary>
        /// Событие выбора Кредита
        /// </summary>
        private void Credit_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonCapitalization.Visibility = Visibility.Hidden;
            Title = "Новый кредит";
            PercentBox.Text = main.bank.GetActualCreditPercent(client).ToString();
        }
    }
}
