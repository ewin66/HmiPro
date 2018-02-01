using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.ViewModels.Sys.Form {
    /// <summary>
    /// 需要输入密码的Form
    /// <author>ychost</author>
    /// <date>2018-1-27</date>
    /// </summary>
    public class PasswordForm : BaseForm {
        [Display(Name = "密码"), DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
