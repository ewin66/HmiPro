using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.ViewModels.Sys.Form {
    /// <summary>
    /// 去往测试界面需要密码
    /// <author>ychost</author>
    /// <date>2018-1-27</date>
    /// </summary>
    public class NavToTestViewForm {
        [Display(Name = "密码"), DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
