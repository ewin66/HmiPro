﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using HmiPro.Annotations;

namespace HmiPro.ViewModels.DMes.Tab {

    public abstract class BaseTab:INotifyPropertyChanged  {

        private string header;

        public string Header {
            get { return header; }
            set {
                if (value != header) {
                    header = value;
                    RaisePropertyChanged(nameof(Header));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
