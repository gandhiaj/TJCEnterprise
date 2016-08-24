using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TJC.AppLink.BusinessEntity.ClassLibrary.Classes;
using TJC.AppLink.DAL.ClassLibrary;
using TJC.AppLink.DAL.ClassLibrary.Interfaces;
using TJC.AppLink.Models.ClassLibrary;

namespace TJC.AppLink.BusinessEntity.ClassLibrary
{
    public class AppLinkBusiness : BaseManager, IAppLinkManager
    {
        private bool _disposed;

        public List<AppLinkModel> GetAppLinkInformation(string sUserId)
        {
            var oAppLink = new AppLinkRepository();
            return oAppLink.GetAppLinkInformation(sUserId);
        }

        #region IDisposable Members

        /// <summary>
        ///   Perform application-defined tasks associated with freeing, releasing, 
        ///   or resetting resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Perform application-defined tasks associated with freeing, releasing, 
        ///   or resetting resources
        /// </summary>
        /// <param name = "disposing">If disposing equals true, the method has been called directly
        ///   or indirectly by a user's code. Managed and unmanaged resources
        ///   can be disposed.
        ///   If disposing equals false, the method has been called by the
        ///   runtime from inside the finalizer and you should not reference
        ///   other objects. Only unmanaged resources can be disposed.</param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (_unitofWork != null)
                    {
                        _unitofWork = null;
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        #endregion
    }
}
