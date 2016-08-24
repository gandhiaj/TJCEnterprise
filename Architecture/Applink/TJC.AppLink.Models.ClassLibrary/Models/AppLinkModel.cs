using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace TJC.AppLink.Models.ClassLibrary
{
    public class AppLinkModel
    {
        [Key]
        [DisplayName("User Login Code")]
        public string usr_lgn_cd { get; set; }

        [DisplayName("Application Code")]
        public string apl_cd { get; set; }

        [DisplayName("Application Name")]
        public string apl_nm { get; set; }

        [DisplayName("Application URL")]
        public string apl_url_tx { get; set; }

        [DisplayName("Application Type")]
        public string apl_type_ind { get; set; }
    }
}
