using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Annotations;
using HmiPro.Config;
using HmiPro.ViewModels.DMes;

namespace HmiPro.ViewModels.Sys {
    /// <summary>
    /// 导航条
    /// <author>ychost</author>
    /// <date>2017-12-27</date>
    /// </summary>
    [POCOViewModel]
    public class NavigatorViewModel {
        /// <summary>
        /// 导航条内容，都是机台编码
        /// </summary>
        public virtual ObservableCollection<Navigator> Navigators { get; set; } = new ObservableCollection<Navigator>();
        public virtual INavigationService NavigationSerivce => null;


        public NavigatorViewModel() {
            foreach (var pair in MachineConfig.MachineDict) {
                var nav = new Navigator() {
                    MachineCode = pair.Key,
                    Url = pair.Key
                };
                if (nav.MachineCode == App.Store.GetState().ViewStoreState.NavView.DMesSelectedMachineCode) {
                    nav.IsSelected = true;
                }
                Navigators.Add(nav);
            }
        }

    }

    public class Navigator : INotifyPropertyChanged {
        public string MachineCode { get; set; }
        public string Url { get; set; }
        private bool isSelected;

        public bool IsSelected {
            get => isSelected;
            set {
                if (isSelected != value) {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}