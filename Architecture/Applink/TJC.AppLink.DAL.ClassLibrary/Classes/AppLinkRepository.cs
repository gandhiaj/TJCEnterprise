using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.ServiceLocation;
using TJC.AppLink.DAL.ClassLibrary.Interfaces;
using System.Data.SqlClient;
using TJC.AppLink.Models.ClassLibrary;

namespace TJC.AppLink.DAL.ClassLibrary
{
    public class AppLinkRepository : IAppLinkRepository
    {
        public List<AppLinkModel> GetAppLinkInformation(string sUserId)
        {
            var parameters = new object[] { new SqlParameter("@usr_lgn_cd", sUserId) };
            var result = prcContext.ExecuteStoredProc<AppLinkModel>("prc_apl_link_slc_info @usr_lgn_cd", parameters);
            if (result != null)
            {
                return result.ToList<AppLinkModel>();
            }
            else
                return null;
        }

        public AppLinkRepository()
        {
            prcContext = ServiceLocator.Current.GetInstance<IGenericRepository>("DBPRC01");
        }

        /// <summary>
        /// Stored Proc DBContext
        /// </summary>
        public IGenericRepository prcContext
        {
            get;
            set;
        }
    }
}
