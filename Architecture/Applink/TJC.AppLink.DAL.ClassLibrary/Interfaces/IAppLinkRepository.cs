using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TJC.AppLink.Models.ClassLibrary;

namespace TJC.AppLink.DAL.ClassLibrary.Interfaces
{
    public interface IAppLinkRepository
    {
        IGenericRepository prcContext { get; }

        List<AppLinkModel> GetAppLinkInformation(string sUserId);
    }
}
