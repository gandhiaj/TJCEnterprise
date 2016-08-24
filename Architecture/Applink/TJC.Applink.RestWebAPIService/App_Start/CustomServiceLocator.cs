using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ServiceLocation;
using TJC.AppLink.Models.ClassLibrary;
using TJC.AppLink.BusinessEntity.ClassLibrary;
using TJC.AppLink.DAL.ClassLibrary.Interfaces;
using TJC.AppLink.DAL.ClassLibrary;
using TJC.AppLink.DAL.ClassLibrary.Classes;

namespace TJC.Applink.RestWebAPIService.App_Start
{
    public class CustomServiceLocator
    {
        public static void Configure()
        {
            string _user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("JCAHO\\", " - ");
            UnityContainer uc = new UnityContainer();
            uc.RegisterType<IAppLinkRepository, AppLinkRepository>();
            uc.RegisterType<IGenericRepository, GenericRepository>("DBPRC01", new InjectionConstructor(new DBPRC01()));
            uc.RegisterType<IAppLinkManager, AppLinkBusiness>();
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(uc));
        }
    }
}