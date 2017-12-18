using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using HmiPro.Config.Models;
using Microsoft.Win32;

namespace HmiPro.ViewModels.Sys {

    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class SettingViewModel {
        public virtual Setting Setting { get; set; }

        public SettingViewModel()
        {
            
        }

        public SettingViewModel(Setting setting) {
            this.Setting = setting;
        }

        [Command(Name = "ChooseMachineXlsPathCommand")]
        public void ChooseMachineXlsPath() {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xls文件|*.xls";
            if (ofd.ShowDialog() == false) {
                return;
            }
            Setting.MachineXlsPath = ofd.FileName;
        }

        /// <summary>
        /// 这是个JumpView需要在其它视图中手动调用该初始化方法
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static SettingViewModel Create(Setting setting) {
            return ViewModelSource.Create(() => new SettingViewModel(setting));
        }

    }
}