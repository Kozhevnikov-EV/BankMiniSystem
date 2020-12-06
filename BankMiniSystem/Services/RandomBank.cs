using System;
using BankModel_Library;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMiniSystem.Services
{
    /// <summary>
    /// Статический класс, метод которого создает и возвращает экземпляр Bank со случайным набором свойств
    /// </summary>
    public static class RandomBank
    {
        /// <summary>
        /// Свойство - рандом
        /// </summary>
        private static Random R = new Random();

        /// <summary>
        /// Метод создания экземпляра банка, наполненного случайными клиентами и счетами
        /// </summary>
        /// <returns>Bank</returns>
        internal static Bank CreateRandomBank()
        {
            Bank bank = new Bank("Банк сочувствие", 12);
            Account.ResetStaticId();
            Client.ResetStaticId();
            Transaction.ResetStaticId();
            for (int i = 1; i <= 100; i++) //Добавляем по одному экземпляру клиента каждого класса наследника Client
            {
                bank.AddCompany($"OOO", $"Организация_{i}");
                bank.AddVIP($"Имя_{R.Next(1,101)}", $"Фамилия_{R.Next(1, 101)}", new DateTime(1980, 01, 01), $"Предприятие_{R.Next(1, 101)}");
                bank.AddNatural($"Имя_{R.Next(1, 101)}", $"Фамилия_{R.Next(1, 101)}", new DateTime(1980, 01, 01));
            }
            foreach (var client in bank.Clients) //добавляем каждому клиентов случайное число счетов со случайными суммами на балансе
            {
                for (int i = 0; i < R.Next(1, 100); i++) { bank.AddAccount(client.Id, Convert.ToDouble(R.Next(1,11)*100)); }
            }
            return bank;
        }
    }
}
