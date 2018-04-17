using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.Func {
    [POCOViewModel]
    public class WorkMgmtViewModel {
        public virtual ObservableCollection<Employee> Employees { get; set; }
        public virtual Employee SelectEmployee { get; set; }

        public WorkMgmtViewModel() {
            var emp = new Employee() {
                Name = "雏田",
                Phone = "18380461616",
                PrintCardTime = DateTime.Now,
                Photo = @"https://ss3.bdstatic.com/70cFv8Sh_Q1YnxGkpoWK1HF6hhy/it/u=259945095,4151803078&fm=27&gp=0.jpg"
            };
            var emp2 = new Employee() {
                Name = "鸣人",
                Photo =
                    @"https://ss0.bdstatic.com/70cFuHSh_Q1YnxGkpoWK1HF6hhy/it/u=2504762887,2168301079&fm=27&gp=0.jpg",
                PrintCardTime = DateTime.Now,
                Phone = "185478751266"
            };
            Employees = new ObservableCollection<Employee>();
            Employees.Add(emp2);
            Employees.Add(emp);

        }
    }
}