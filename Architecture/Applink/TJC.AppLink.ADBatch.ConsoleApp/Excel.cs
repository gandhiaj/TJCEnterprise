using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
//using Microsoft.Office.Interop.Excel;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;

namespace TJC.AppLink.ADBatch.ConsoleApp
{
    class Excel
    {
        //static void Main(string[] args)
        //{
        //    Application excelApp = new Application();
        //    string myPath = @"C:\Users\gandhia\Documents\ADUsers.xlsx";
        //    excelApp.Workbooks.Open(myPath);

        //    // Get Worksheet
        //    Worksheet worksheet = excelApp.Worksheets[1];
        //    int rowIndex = 1;

        //    string[] separatingChars = { "|" };
        //    string[] ADGroups = ConfigurationManager.AppSettings["ADGroups"].ToString().Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);

        //    foreach (var group in ADGroups)
        //    {
        //        using (var pcontext = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings["DomainAccount"], group))
        //        {
        //            UserPrincipal userQuery = new UserPrincipal(pcontext);

        //            // get the enabled users only
        //            userQuery.Enabled = true;

        //            using (var searcher = new PrincipalSearcher())
        //            {
        //                searcher.QueryFilter = userQuery;

        //                foreach (var result in searcher.FindAll())
        //                {
        //                    DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

        //                    if (de.Properties["employeeNumber"].Value != null && de.Properties["company"].Value != null && de.Properties["department"].Value != null)
        //                    {

        //                        rowIndex = rowIndex + 1;

        //                        worksheet.Cells[1][rowIndex].Value = de.Properties["givenName"].Value;
        //                        worksheet.Cells[2][rowIndex].Value = de.Properties["sn"].Value;
        //                        worksheet.Cells[3][rowIndex].Value = de.Properties["company"].Value;
        //                        worksheet.Cells[4][rowIndex].Value = de.Properties["department"].Value;
        //                        worksheet.Cells[5][rowIndex].Value = de.Properties["employeeNumber"].Value;
        //                        worksheet.Cells[6][rowIndex].Value = de.Properties["distinguishedName"].Value;

        //                    }
        //                }
        //            }
        //        }
        //    }

        //    worksheet.SaveAs(@"C:\Users\gandhia\Documents\ADUsers.xlsx");
        //    excelApp.Workbooks.Close();
        //}

    }
}
