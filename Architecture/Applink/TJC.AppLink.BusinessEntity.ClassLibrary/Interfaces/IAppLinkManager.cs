﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TJC.AppLink.Models.ClassLibrary;

namespace TJC.AppLink.BusinessEntity.ClassLibrary
{
    public interface IAppLinkManager : IDisposable
    {
        List<AppLinkModel> GetAppLinkInformation(string sUserId);
    }
}
