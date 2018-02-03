using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using HmiPro.Annotations;

namespace HmiPro.ViewModels.DMes.Tab {

    /// <summary>
    /// DMesCoreView 的基础页面
    /// <author>ychost</author>
    /// <date>2017-12-18</date>
    /// </summary>
    public abstract class BaseTab:INotifyPropertyChanged  {

        private string header;
        /// <summary>
        /// 一个页面上面的标题
        /// </summary>
        public string Header {
            get { return header; }
            set {
                if (value != header) {
                    header = value;
                    RaisePropertyChanged(nameof(Header));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 就是 OnPropertyChanged 
        /// </summary>
        /// <param name="propertyName"></param>
        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
