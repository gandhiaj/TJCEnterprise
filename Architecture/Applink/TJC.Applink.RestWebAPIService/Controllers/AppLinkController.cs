using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TJC.AppLink.BusinessEntity.ClassLibrary;

namespace TJC.Applink.RestWebAPIService.Controllers
{
    public class AppLinkController : ApiController
    {

        public HttpResponseMessage GetAppLinkInformation(string Id)
        {
            HttpResponseMessage response = null;
            try
            {                
                var oAESDecrypt = new AESDecrypt();              
                var oAppLink = new AppLinkBusiness();
                var result = oAppLink.GetAppLinkInformation(oAESDecrypt.DecryptStringAES(Id));
                if (result == null || !result.Any())
                {
                    response = Request.CreateResponse(HttpStatusCode.NotFound, "No Data found");
                }
                else
                    response = Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                // Log the message 
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
            return response;
        }

    }
}
