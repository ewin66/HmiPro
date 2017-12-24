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

    public abstract class BaseTab  {

        private string header;

        public string Header {
            get { return header; }
            set {
                if (value != header) {
                    header = value;
                    OnPropertyChanged(nameof(Header));

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
