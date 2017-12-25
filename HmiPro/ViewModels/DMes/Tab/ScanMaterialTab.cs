using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.ViewModels.DMes.Tab {
    /// <summary>
    /// 扫描来料界面
    /// <date>2017-12-25</date>
    /// <author>ychost</author>
    /// </summary>
    public class ScanMaterialTab : BaseTab {
        private MqScanMaterial mqScanMaterial;

        public MqScanMaterial MqScanMaterial {
            get => mqScanMaterial;
            set {
                if (mqScanMaterial != value) {
                    mqScanMaterial = value;
                    OnPropertyChanged(nameof(MqScanMaterial));
                }
            }
        }

        public void Update(MqScanMaterial material) {
            MqScanMaterial = material;
        }

    }
}
