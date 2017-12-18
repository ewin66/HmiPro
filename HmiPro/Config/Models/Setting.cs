using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.Config.Models {
    /// <summary>
    /// 缓存在sqlite中的配置
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public class Setting : INotifyPropertyChanged {
        public Setting() {
            UpdateTime = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        private string machineXlsPath;
        /// <summary>
        /// 机台xls配置文件地址
        /// </summary>
        public string MachineXlsPath {
            get => machineXlsPath;
            set {
                if (machineXlsPath != value) {
                    machineXlsPath = value;
                    OnPropertyChanged(nameof(MachineXlsPath));
                }
            }
        }


        public DateTime UpdateTime { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
