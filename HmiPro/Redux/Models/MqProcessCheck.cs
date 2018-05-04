
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm;
using HmiPro.Annotations;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// 制程质检
    /// <author>ychost</author>
    /// <date>2018-4-25</date>
    /// </summary>
    public class MqProcessCheck : INotifyPropertyChanged {
        /// <summary>
        /// 工序编号
        /// </summary>
        public string seqCode { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        public string seqName { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string proGgxh { get; set; }

        /// <summary>
        /// 检查项
        /// </summary>
        public string detectionItem { get; set; }

        /// <summary>
        /// 检测标准
        /// </summary>
        public string produceCod { get; set; }

        /// <summary>
        /// 格式，input/select
        /// </summary>
        public string produceType { get; set; }

        /// <summary>
        /// select 参数以 ',' 隔开
        /// </summary>
        public string selectParam { get; set; }

        /// <summary>
        /// select 选项数组
        /// </summary>
        public string[] selectParamArr { get; set; }

        /// <summary>
        /// 检测结果
        /// </summary>
        public string produceResult { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }


        //--------------回传---------------
        /// <summary>
        /// 机台编号
        /// </summary>
        public string macCode { get; set; }
        /// <summary>
        /// 班次，白班，夜班
        /// </summary>
        public string classType { get; set; }
        /// <summary>
        /// 工单
        /// </summary>
        public string workCode { get; set; }
        /// <summary>
        /// 是否合格，合格/不合格
        /// </summary>
        public string pass { get; set; }

        public int checkTimes { get; set; } = 1;


        public string[] passSelect { get; set; }

        public MqProcessCheck() {
            passSelect = new[] { "合格", "不合格" };
            //    pass = passSelect[0];
            //    if (selectParamArr?.Length > 0) {
            //        produceResult = selectParamArr[0];
            //    }
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
