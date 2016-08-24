using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;
using System.Collections;

namespace TJC.AppLink.ADBatch.ConsoleApp
{
    class Program
    {
        static ClientContext context;

        static List ADList;
        static ListItemCollection items;
        static CamlQuery CAMLQuery = new CamlQuery();

        static void Main(string[] args)
        {
            context = new ClientContext(ConfigurationManager.AppSettings["SharepointUrl"]);
            
            GetADUsers();

            string[] separatingChars = { "|" };
            string[] ADGroups = ConfigurationManager.AppSettings["ADGroups"].ToString().Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var group in ADGroups)
            {
                using (var pcontext = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings["DomainAccount"], group))
                {
                    UserPrincipal userQuery = new UserPrincipal(pcontext);

                    // get the enabled users only
                    userQuery.Enabled = true;

                    using (var searcher = new PrincipalSearcher())
                    {
                        searcher.QueryFilter = userQuery;

                        foreach (var result in searcher.FindAll())
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                            if (de.Properties["employeeNumber"].Value != null && de.Properties["company"].Value != null && de.Properties["department"].Value != null) //|| (de.Properties["distinguishedName"].Value.ToString().Contains("OU=consultants,OU=it,OU=PC") || de.Properties["distinguishedName"].Value.ToString().Contains("OU=FADA Surveyors") || de.Properties["distinguishedName"].Value.ToString().Contains("OU=CBA Surveyors"))
                            {
                                var item = from listitem in items.AsEnumerable()
                                           where listitem["UserLgnCode"].ToString().ToLower() == de.Properties["cn"].Value.ToString().ToLower()
                                           select listitem;

                                if (item != null)
                                {
                                    if (item.Count() > 0)
                                    {
                                        // update the ADGroupName in SharePoint list                                                
                                        CAMLQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='UserLgnCode' /><Value Type='Text'>" + de.Properties["cn"].Value.ToString().ToLower().Trim() + "</Value></Eq></Where></Query></View>";
                                        ListItemCollection col = ADList.GetItems(CAMLQuery);
                                        context.Load(col);
                                        context.ExecuteQuery();

                                        if (col.Count > 0)
                                        {
                                            ListItem listItem = col[0];
                                            listItem["ADGroupName"] = CalculateADGroup(de.Properties["company"].Value.ToString().ToUpper().Trim(), de.Properties["department"].Value.ToString().ToUpper().Trim());
                                            listItem["_DCDateModified"] = DateTime.Now;
                                            listItem.Update();
                                        }

                                    }
                                    else
                                    {
                                        // add the user to the SharePoint list
                                        ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                                        ListItem newItem = ADList.AddItem(itemCreateInfo);
                                        newItem["UserLgnCode"] = de.Properties["cn"].Value.ToString().ToLower().Trim();
                                        newItem["ADGroupName"] = CalculateADGroup(de.Properties["company"].Value.ToString().ToUpper().Trim(), de.Properties["department"].Value.ToString().ToUpper().Trim());
                                        newItem["_DCDateModified"] = DateTime.Now;
                                        newItem.Update();
                                    }
                                }
                                else
                                {
                                    // add the user to the SharePoint list
                                    ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                                    ListItem newItem = ADList.AddItem(itemCreateInfo);
                                    newItem["UserLgnCode"] = de.Properties["cn"].Value.ToString().ToLower().Trim();
                                    newItem["ADGroupName"] = CalculateADGroup(de.Properties["company"].Value.ToString().ToUpper().Trim(), de.Properties["department"].Value.ToString().ToUpper().Trim());
                                    newItem["_DCDateModified"] = DateTime.Now;
                                    newItem.Update();
                                }
                            }
                        }
                    }
                }

                context.ExecuteQuery();

            }

            DeleteDisabledUsers();
        }

        static void GetADUsers()
        {
            SecureString passWord = new SecureString();
            foreach (char c in ((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("secureAppSettings"))["UserPwd"].ToCharArray()) passWord.AppendChar(c);
            context.Credentials = new SharePointOnlineCredentials(ConfigurationManager.AppSettings["UserAccount"], passWord);
            
            ADList = context.Web.Lists.GetByTitle(ConfigurationManager.AppSettings["SharePointList"]);
            
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            items = ADList.GetItems(query);

            context.Load(items);
            context.ExecuteQuery();            
        }

        static void DeleteDisabledUsers()
        {
            SecureString passWord = new SecureString();
            foreach (char c in ((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("secureAppSettings"))["UserPwd"].ToCharArray()) passWord.AppendChar(c);
            context.Credentials = new SharePointOnlineCredentials(ConfigurationManager.AppSettings["UserAccount"], passWord);

            ADList = context.Web.Lists.GetByTitle(ConfigurationManager.AppSettings["SharePointList"]);

            CamlQuery query = new CamlQuery();
            query.ViewXml = "<View><Query><Where><Lt><FieldRef Name='_DCDateModified'/><Value Type='DateTime'><Today /></Value></Lt></Where></Query></View>";
            ListItemCollection items = ADList.GetItems(query);

            context.Load(items);
            context.ExecuteQuery();

            foreach (ListItem item in items.ToList())
            {
                item.DeleteObject();
            }

            context.ExecuteQuery();
           
        }

        static string CalculateADGroup(string sCompany, string sDepartment)
        {
            string ADGroupName = string.Empty;

            switch (sCompany)
            {
                case "JC": // TJC Central Office Staff
                    ADGroupName = sCompany + "-CO"; 
                    break;
                case "CTH": // CTH Staff
                    ADGroupName = sCompany;
                    break;
                case "FDJC":
                    switch (sDepartment)
                    {
                        case "DC": // Washington DC Staff
                            ADGroupName = sDepartment;
                            break;
                        default: // TJC Surveyors/Reviewers Staff
                            ADGroupName = sCompany + "-SVY";
                            break;
                    }
                    break;
                case "JCR":
                    switch (sDepartment)
                    {
                        case "INL*": // JCI Central Office Staff
                            ADGroupName = "JCI-CO";
                            break;
                        default:  // JCR Central Office Staff
                            ADGroupName = sCompany + "-CO";
                            break;
                    }
                    break;
                case "FDJCR":
                    switch (sDepartment)
                    {
                        case "INLACR":  // JCI Surveyors/Reviewers Staff
                            ADGroupName = "JCI-SVY";
                            break;
                        default: // JCI/R Consulting Staff
                            ADGroupName = "JCI/R-CONSULTING"; 
                            break;
                    }
                    break;
                case "INTL":
                    switch (sDepartment)
                    {
                        case "INLACR":  // JCI Surveyors/Reviewers Staff
                            ADGroupName = "JCI-SVY";
                            break;
                        default: // JCI/R Consulting Staff
                            ADGroupName = "JCI/R-CONSULTING";
                            break;
                    }
                    break;
            }

            return ADGroupName;
        }
    }
}
