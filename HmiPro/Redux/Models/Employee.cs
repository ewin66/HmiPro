using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Annotations;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 操作员工
    /// <author>ychost</author>
    /// <date>2018-4-17</date>
    /// </summary>
    public class Employee : INotifyPropertyChanged {
        /// <summary>
        /// 人员关联的 Rfid
        /// </summary>
        public string Rfid { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 打卡机台
        /// </summary>
        public string MachineCode { get; set; }

        private string photo;
        /// <summary>
        /// 照片
        /// </summary>
        public string Photo {
            get => photo;

            set {
                if (photo != value) {
                    photo = value;
                    OnPropertyChanged(nameof(Photo));
                }
            }
        }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public DateTime PrintCardTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
