using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;

namespace HmiPro.ViewModels.Sys {
    [POCOViewModel]
    public class TestViewModel {
        public virtual ObservableCollection<SelectableViewModel> Items3 { get; set; }
        public TestViewModel() {
            Items3 = CreateData();
        }

        private static ObservableCollection<SelectableViewModel> CreateData() {
            return new ObservableCollection<SelectableViewModel>
            {
                new SelectableViewModel
                {
                    Code = 'M',
                    Name = "Material Design",
                    Description = "Material Design in XAML Toolkit"
                },
                new SelectableViewModel
                {
                    Code = 'D',
                    Name = "Dragablz",
                    Description = "Dragablz Tab Control",
                    Food = "Fries"
                },
                new SelectableViewModel
                {
                    Code = 'P',
                    Name = "Predator",
                    Description = "If it bleeds, we can kill it"
                }
            };
        }
    }
}