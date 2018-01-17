using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 用于输入的参数
    /// <author>ychost</author>
    /// <date>2018-1-17</date>
    /// </summary>
    public class Dpm : INotifyPropertyChanged {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        private string value;

        public string SpecCode { get; set; }

        public string Value {
            get => value;
            set {
                if (this.value != value) {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dpm() {
            Time = DateTime.Now;
        }
    }

    public enum DpmValueType {
        String,
        Float,
    }
}
