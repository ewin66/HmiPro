using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraExport.Xls;
using HmiPro.Annotations;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// Oee的值，Oee由时间、速度、质量效率三部分组成
    /// 更新其中的一项都会自动更新Oee的值
    /// <author>ychost</author>
    /// <date>2017-12-20</date>
    /// </summary>
    public class Oee : INotifyPropertyChanged {

        public Oee() {
            TimeEff = 1;
            SpeedEff = 1;
            QualityEff = 1;
        }

        private float timeEff;

        public float TimeEff {
            get => timeEff;
            set {
                if (timeEff != value) {
                    timeEff = value;
                    OnPropertyChanged(nameof(TimeEff));
                    UpdateOeeVal();
                }
            }
        }

        private float speedEff;

        public float SpeedEff {
            get { return speedEff; }
            set {
                if (speedEff != value) {
                    speedEff = value;
                    OnPropertyChanged(nameof(SpeedEff));
                    UpdateOeeVal();
                }
            }
        }

        private float qualityEff;

        public float QualityEff {
            get { return qualityEff; }
            set {
                if (qualityEff != value) {
                    qualityEff = value;
                    OnPropertyChanged(nameof(QualityEff));
                    UpdateOeeVal();
                }
            }
        }

        /// <summary>
        /// 更新Oee的值
        /// </summary>
        public void UpdateOeeVal() {
            OeeVal = TimeEff * SpeedEff * QualityEff;
        }


        private float oeeVal;
        /// <summary>
        /// 
        /// </summary>
        public float OeeVal {
            get { return oeeVal; }
            set {
                if (oeeVal != value) {
                    oeeVal = value;
                    OnPropertyChanged(nameof(OeeVal));
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
