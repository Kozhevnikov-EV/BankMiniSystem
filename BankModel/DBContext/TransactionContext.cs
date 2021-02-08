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
    /// Класс контекста для Transaction
    /// </summary>
    public class TransactionContext : DbContext
    {
        public TransactionContext() : base("DbConnection") { }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
