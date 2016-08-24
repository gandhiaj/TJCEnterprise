using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using TJC.AppLink.Models.ClassLibrary;

namespace TJC.AppLink.DAL.ClassLibrary.Classes
{
    public class DBPRC01 : DbContext
    {
        static DBPRC01()
        {
            Database.SetInitializer<DBPRC01>(null);
        }

        public DbSet<AppLinkModel> GetAppLinkInformation { get; set; }

    }
}
