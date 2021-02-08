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
    /// Класс контекста для BankSettings
    /// </summary>
    public class BankSettingsContext : DbContext
    {
        public BankSettingsContext() : base("DbConnection") { }

        public DbSet<BankSettings> Settings { get; set; }
    }
}
