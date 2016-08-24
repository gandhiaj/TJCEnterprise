using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TJC.AppLink.DAL.ClassLibrary.Interfaces;
using TJC.AppLink.Models.ClassLibrary;
using Microsoft.Practices.ServiceLocation;

namespace TJC.AppLink.BusinessEntity.ClassLibrary.Classes
{
    public abstract class BaseManager
    {
        protected IAppLinkRepository _unitofWork;

        protected BaseManager()
        {
            _unitofWork = this.ResolveDataFactory<IAppLinkRepository>();
        }

        /// <summary>
        /// Return new instance of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveDataFactory<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

        /// <summary>
        /// Saves DbContext and commits to Database
        /// </summary>
        protected virtual void Save(IGenericRepository context)
        {
            //    //Calling save from any repository will save entire db context. 
            //    //No need to issue save for each repository. 
            context.Save();
        }
    }
}
