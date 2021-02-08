using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BankModel_Library;

namespace BankModel_Library
{
    /// <summary>
    /// Класс контекста для Account, Credit, Deposit
    /// </summary>
    public class AccountContext: DbContext
    {
        public AccountContext() : base("DbConnection")
        { }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Credit> Credits { get; set; }

        public DbSet<Deposit> Deposits { get; set; }
    }
}
