using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankModel_Library
{
    interface IClient
    {
        /// <summary>
        /// Имя/название
        /// </summary>
        string Name { get; }
    }
}
