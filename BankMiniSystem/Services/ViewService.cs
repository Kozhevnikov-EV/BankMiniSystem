using System;
using BankModel_Library;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BankMiniSystem.Services
{
    /// <summary>
    /// Набор статических методов для подготовки коллекций и пр. элементов, которые привязаны к элементам xaml
    /// </summary>
    class ViewService
    {
        /// <summary>
        /// Наполняет переданную коллекцию clients экземплярами <T> из коллекции bank.clients
        /// </summary>
        /// <typeparam name="T">Унаследованный от Client класс</typeparam>
        /// <param name="bank">Экземпляр банка, хранящий всех клиентов в коллекции bank.clients</param>
        /// <param name="clients">Коллекция клиентов, которую необходимо наполнить экземплярами T из коллекции bank.clients</param>
        internal static void CreateClientCol<T>(Bank bank, ObservableCollection<T> clients)
            where T : Client
        {
            if (bank != null) //проверка, что переданный экземпляр банка не нул
            {
                clients.Clear(); //очищаем коллекцию
                foreach (var client in bank.Clients) //собираем коллекцию
                {
                    if (client.GetType() == typeof(T)) { clients.Add((T)client); } //добавляем в нее экземпляры с типом T
                }
            }
        }

        /// <summary>
        /// Наполняет переданную коллекцию account экземплярами <T> из коллекции bank.accounts, принадлежащих client
        /// </summary>
        /// <typeparam name="T">Унаследованный от Account класс</typeparam>
        /// <param name="bank">Экземпляр банка, хранящий все счета в коллекции bank.accounts</param>
        /// <param name="accounts">Коллекция счетов, которую необходимо наполнить</param>
        /// <param name="client">Экземпляр клиента, для которого необходимо найти все счета</param>
        internal static void CreateAccountCol<T>(Bank bank, ObservableCollection<T> accounts, Client client)
            where T : Account
        {
            if (bank != null && client != null)
            {
                accounts?.Clear();
                foreach (var account in bank.Accounts)
                {
                    if (account.ClientId == client.Id) { accounts.Add((T)account); }
                }
            }
        }

        /// <summary>
        /// Наполняет переданную коллекцию account экземплярами из коллекции bank.accounts, принадлежащих client
        /// Имеющих статус Open и без классов наследников (без кредитов и вкладов)
        /// </summary>
        /// <param name="bank">Экземпляр банка, хранящий все счета в коллекции bank.accounts</param>
        /// <param name="accounts">Коллекция счетов, которую необходимо наполнить</param>
        /// <param name="client">Экземпляр клиента, для которого необходимо найти все открытые счета</param>
        internal static void CreateOpenAccountCol(Bank bank, ObservableCollection<Account> accounts, Client client)
        {
            if (bank != null && client != null)
            {
                accounts?.Clear();
                foreach (var account in bank.Accounts)
                {
                    if (account.ClientId == client.Id && account.IsOpen && account.GetType() == typeof(Account)) { accounts.Add(account); }
                }
            }
        }

        /// <summary>
        /// Проверяет добавляемый символ к переменной TextInField:
        /// 1) Символ может являться числом
        /// 2) Символ может являться десятичным разделителем ','
        /// 3) После добавления символа к переменной TextInField она будет являться десятичным числом формата 000,00 или 000,0 или целым
        /// </summary>
        /// <param name="InputChar">Вводимый символ</param>
        /// <param name="TextInField">Строковая переменная с текущим введенным числом</param>
        /// <returns>true - если при вводе символа InputChar TextInField не будет иметь необходимый формат или
        /// вводимый символ не число и не десятичный разделитель ','</returns>
        internal static bool ValidateInputDecimalNumber(char InputChar, string TextInField)
        {
            bool NotValidate = false;
            if (TextInField.Contains(',') && Char.IsDigit(InputChar)) //проверяем, что вводимый символ число и уже есть десятичный разделитель
            {
                if ((TextInField.Length - TextInField.IndexOf(',')) > 2) //если число символов после разделителя ',' больше 2, то прерываем ввод
                {
                    NotValidate = true;
                }
            }
            else if (!Char.IsDigit(InputChar) && InputChar != ',') //далее, если вводимый сивол не цифра и не разделитель, то прерываем ввод
            {
                NotValidate = true;
            }
            else if (InputChar == ',' && TextInField.Contains(',')) //если вводимый симсвол запятая, то проверям, нет ли ее уже в введенном тексте
            {
                NotValidate = true; //если есть, то прерываем ввод
            }
            return NotValidate;
        }

        /// <summary>
        /// Возвращает коллекцию всех транзакций по счету account
        /// </summary>
        /// <param name="bank">Экземпляр Bank, хранящий коллекцию всех транзакций</param>
        /// <param name="account">Счет, по которому необходимо вывести все транзакции</param>
        /// <returns>ObservableCollection<Transaction></returns>
        public static ObservableCollection<Transaction> GetTransactionsOfAccount(Bank bank, Account account)
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
            foreach(var e in bank.Transactions)
            {
                if(e.FromAccount == account.Id || e.ToAccount == account.Id) { transactions.Add(e); }
            }
            return transactions;
        }

        public static void GetLogCollection(Bank bank, ObservableCollection<ActivityInfo> activities)
        {
            activities?.Clear();
            foreach(var act in bank.LogList)
            {
                activities.Add(act);
            }
        }
    }
}