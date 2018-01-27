using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using HmiPro.Annotations;

namespace HmiPro.ViewModels.Sys {
    /// <summary>
    /// 表单视图模型
    /// <author>ychost</author>
    /// <date>2018-1-24</date>
    /// </summary>
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class FormViewModel {
        public virtual object Form { get; set; }
        public virtual string Title { get; set; }

        public FormViewModel() {

        }

        public FormViewModel(string title, object form) {
            Title = title;
            Form = form;
        }

        public static FormViewModel Create(string title,object formCtrls) {
            return ViewModelSource.Create(() => new FormViewModel(title,formCtrls));
        }

    }


    public class BaseFormCtrl {
        //The two items below will be displayed by DataLayoutControl
        // in a borderless and titleless Name group 
        [Display(GroupName = "<Name>", Name = "Last name")]
        public string LastName { get; set; }
        [Display(GroupName = "<Name>", Name = "First name", Order = 0)]
        public string FirstName { get; set; }

        //The four items below will go to a Contact tab within tabbed Tabs group. 
        [Display(GroupName = "{Tabs}/Contact", Order = 2),
         DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Display(GroupName = "{Tabs}/Contact", Order = 4)]
        public string Email { get; set; }
        //The two items below will go to the Address group within the Contact tab. 
        [Display(GroupName = "{Tabs}/Contact/Address", ShortName = "")]
        public string AddressLine1 { get; set; }
        [Display(GroupName = "{Tabs}/Contact/Address", ShortName = "")]
        public string AddressLine2 { get; set; }

        //The two items below will go to the horizontally oriented Personal group. 
        [Display(GroupName = "Personal-", Name = "Birth date")]
        public DateTime BirthDate { get; set; }

        //The four items below will go to the Job tab of the tabbed Tabs group 
        [Display(GroupName = "{Tabs}/Job", Order = 6)]
        public string Group { get; set; }
        [Display(GroupName = "{Tabs}/Job", Name = "Hire date")]
        public DateTime HireDate { get; set; }
        [Display(GroupName = "{Tabs}/Job"), DataType(DataType.Currency)]
        public decimal Salary { get; set; }
        [Display(GroupName = "{Tabs}/Job", Order = 7)]
        public string Title { get; set; }
    }

}
