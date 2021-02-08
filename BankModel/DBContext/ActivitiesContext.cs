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
    /// Класс контекста для ActivityInfo и BalanceLog
    /// </summary>
    public class ActivitiesContext : DbContext
    {
        public ActivitiesContext() : base("DbConnection") { }

        public DbSet<ActivityInfo> ActivityInfos { get; set; }

        public DbSet<BalanceLog> BalanceLogs { get; set; }
    }
}
