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
    /// Класс контекста для Client, NaturalPerson, VIP, Company
    /// </summary>
    public class ClientContext : DbContext
    {
        public ClientContext() : base("DbConnection") { }

        public DbSet<Client> Clients { get; set; }

        public DbSet<NaturalPerson> Naturals { get; set; }

        public DbSet<VIP> VIPs { get; set; }

        public DbSet<Company> Companies { get; set; }
    }
}
