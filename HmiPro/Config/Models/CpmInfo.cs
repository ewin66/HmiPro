using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Config.Models {
    /// <summary>
    /// 底层采集参数封装
    /// 主要封装了某些计算方法体，比如平均值，最大值，均方差等等
    /// <date>2017-10-02</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmInfo {
        public int Code;
        public string Name;
        public string Unit;
        public string[] MqAlarmBomKeys;
        /// <summary>
        /// Plc报警配置，可能为值，也可能为值_max，值_min
        /// </summary>
        public string PlcAlarmKey;

        public string[] ExpAlarms;


        private CpmInfoMethodName? methodName;
        public CpmInfoMethodName? MethodName {
            get => methodName;
            set {
                methodName = value;
                switch (value) {
                    //平均值需要迭代算法
                    case CpmInfoMethodName.Average:
                        avgValArr = new float[2];
                        break;
                    //均方差也需要迭代算法
                    case CpmInfoMethodName.StdDev:
                        stdDevValArr = new float[3];
                        break;
                    //根据时间计算导数
                    case CpmInfoMethodName.DervativeByTime:
                        derByTime = new CalcDerByTime();
                        break;
                }
            }
        }



        /// <summary>
        /// 该参数是需要计算的，这是计算所以需要的其他参数
        /// </summary>
        public string MethodParams;
        public List<int> MethodParamInts;
        public List<string> MethodParamStrs;

        //参数算法体
        private float minVal;
        private float maxVal;
        private float[] stdDevValArr;
        private float[] avgValArr;
        //上一个值，求导数用
        //该倒数基于单位长度
        private float? lastValForDerivative;
        private CalcDerByTime derByTime;

        /// <summary>
        /// 通过时间求取倒数值
        /// 该倒数根据时间长度
        /// </summary>
        struct CalcDerByTime {
            //上次的值
            public float? LastValue;
            /// <summary>
            /// 上一次算出的导数值
            /// </summary>
            public float LastDer;
            //上次的时间
            public DateTime LastTime;
            /// <summary>
            /// 采集频率比普通频率快，那么就要消除抖
            /// 这是记录抖动次数，即为 0 的次数
            /// </summary>
            public int ZeroTimes;

        }

        public float? ExecCalc(IDictionary<int, float> codeToValDict, int updateCode) {
            //更新的参数不属于计算依赖的参数
            if (!MethodParamInts.Contains(updateCode)) {
                return null;
            }
            switch (MethodName) {
                case CpmInfoMethodName.Average:
                    return execAvg(codeToValDict);
                case CpmInfoMethodName.Max:
                    return execMax(codeToValDict);
                case CpmInfoMethodName.Min:
                    return execMin(codeToValDict);
                case CpmInfoMethodName.StdDev:
                    return execStdDev(codeToValDict);
                case CpmInfoMethodName.PowerFactory:
                    return execPowerFactory(codeToValDict);
                case CpmInfoMethodName.Reciprocal:
                    return execReciprocal(codeToValDict);
                case CpmInfoMethodName.Derivative:
                    return execDerivative(codeToValDict);
                case CpmInfoMethodName.DervativeByTime:
                    return execDerivativeByTime(codeToValDict);
            }
            return execDefault(codeToValDict);
        }

        /// <summary>
        /// 计算最大值
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execMax(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(paramCodes[0])) {
                return null;
            }
            if (codeToValDict[paramCodes[0]] > maxVal) {
                maxVal = codeToValDict[paramCodes[0]];
            }
            return maxVal;
        }

        /// <summary>
        /// 计算最小值
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execMin(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(paramCodes[0])) {
                return null;
            }

            if (codeToValDict[paramCodes[0]] < minVal) {
                minVal = codeToValDict[paramCodes[0]];
            }
            return minVal;
        }


        /// <summary>
        ///  http://www.mathchina.net/dvbbs/dispbbs.asp?boardid=5&Id=1523
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execStdDev(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(paramCodes[0])) {
                return null;
            }

            //最新的值Xn
            var Xn = codeToValDict[paramCodes[0]];
            //n-1的值
            var n_1 = this.stdDevValArr[0];
            //Xn-1的平均数
            var Xn_1_Avg = stdDevValArr[1];
            //n的值
            var n = n_1 + 1;
            //Sn-1 方差
            var Sn_1Pow2 = this.stdDevValArr[2];

            var Xn_Avg = Xn_1_Avg + (1 / n) * (Xn - Xn_1_Avg);
            //Sn 方差
            var SnPow2 = (n_1 / n) * (Sn_1Pow2 + (1 / n) * Math.Pow(Xn - Xn_1_Avg, 2));

            //更新n-1的内容
            stdDevValArr[0] = n;
            stdDevValArr[1] = Xn_Avg;
            stdDevValArr[2] = (float)SnPow2;
            //返回标准差
            return (float)Math.Pow(SnPow2, 0.5);
        }

        /// <summary>
        /// 计算功率因素
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execPowerFactory(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(paramCodes[0]) || !codeToValDict.ContainsKey(paramCodes[1])) {
                return null;
            }

            //有功功率
            var p = codeToValDict[paramCodes[0]];
            //无功功率
            var q = codeToValDict[paramCodes[1]];
            //功率因素 p/((p^2+q^2)^(1/2))
            var factory = p / (Math.Pow(Math.Pow(p, 2) + Math.Pow(q, 2), (0.5)));
            return (float)factory;
        }

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execAvg(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(paramCodes[0])) {
                return null;
            }
            //Xn的值
            var Xn = codeToValDict[paramCodes[0]];
            //n-1的值
            var n_1 = avgValArr[0];
            //Xn-1的平均值
            var Xn_1_Avg = avgValArr[1];
            //n的值
            var n = n_1 + 1;
            //Xn的平均值
            var Xn_Avg = Xn_1_Avg + (1 / n) * (Xn - Xn_1_Avg);

            //用n的内容更新n-1
            avgValArr[0] = n;
            avgValArr[1] = Xn_Avg;
            return Xn_Avg;
        }

        private float? execDefault(IDictionary<int, float> codeToValDict) {
            if (!codeToValDict.ContainsKey(Code)) {
                return null;
            }
            return codeToValDict[Code];
        }

        /// <summary>
        /// 通过时间计算的倒数，时间单位为秒
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execDerivativeByTime(IDictionary<int, float> codeToValDict) {
            var paramCodes = this.MethodParamInts;
            if (!codeToValDict.ContainsKey(this.MethodParamInts[0])) {
                return null;
            }
            var curVal = codeToValDict[paramCodes[0]];
            var curTime = DateTime.Now;
            //首次只是存储最新值
            if (!derByTime.LastValue.HasValue) {
                derByTime.LastValue = codeToValDict[paramCodes[0]];
                derByTime.LastTime = curTime;
                return null;
            }
            var diffVal = curVal - derByTime.LastValue.Value;
            var diffSec = (curTime - derByTime.LastTime).TotalSeconds;

            //对于负数目前舍去
            if (diffSec > 0 && diffVal >= 0) {
                var derVal = (float)(diffVal / diffSec);
                //突然降为 0 需要消抖
                if (derVal <= 0.01) {
                    if (++derByTime.ZeroTimes <= 10) {
                        return derByTime.LastDer;
                    }
                }
                derByTime.ZeroTimes = 0;
                //消除异常数据和普通抖动数据
                if (derVal > 10 || Math.Abs(derByTime.LastDer - derVal) < 0.05) {
                    return derByTime.LastDer;
                }
                //更新历史值
                derByTime.LastValue = codeToValDict[paramCodes[0]];
                derByTime.LastTime = curTime;
                derByTime.LastDer = derVal;
                return derVal;
            }
            return derByTime.LastDer;
        }


        /// <summary>
        /// 计算导数
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execDerivative(IDictionary<int, float> codeToValDict) {
            var relateCode = this.MethodParamInts[0];
            if (!codeToValDict.ContainsKey(relateCode)) {
                return null;
            }
            var newVal = codeToValDict[relateCode];
            //计算导数，由于是折线变化，x轴的差始终为1
            float? der = codeToValDict[relateCode];
            if (this.lastValForDerivative.HasValue) {
                der = newVal - lastValForDerivative;
            }
            lastValForDerivative = newVal;
            return der;
        }

        /// <summary>
        /// 计算倒数
        /// </summary>
        /// <param name="codeToValDict"></param>
        /// <returns></returns>
        private float? execReciprocal(IDictionary<int, float> codeToValDict) {
            //除数不能为0
            if (codeToValDict.ContainsKey(this.MethodParamInts[0]) && codeToValDict[this.MethodParamInts[0]] - 0.000 >= 0.001) {
                return 1 / codeToValDict[this.MethodParamInts[0]];
            }
            return null;
        }
    }

    public enum CpmInfoMethodName {
        //默认值
        Default = 0,
        //平均值
        Average = 1,
        //最大值
        Max = 2,
        //最小值
        Min = 3,
        //标准差
        StdDev = 4,
        //功率因数
        PowerFactory = 5,
        //转义
        Escape = 6,
        //OEE
        OEE = 7,
        //倒数
        Derivative = 8,
        //倒数
        Reciprocal = 9,
        //时间倒数
        //比如：记米-->速度
        DervativeByTime = 10
    }

    /// <summary>
    /// 采集参数加载器
    /// <date>2017-10-02</date>
    /// <author>ychost</author>
    /// </summary>
    public class CpmLoader {
        private readonly string sheetName;
        private readonly string xlsPath;

        //采集参数的计算参数分隔符
        private readonly string methodParamSplitor;
        public CpmLoader(string xlsPath, string sheetName = "采集参数", string methodParamSplitor = "|") {
            this.xlsPath = xlsPath;
            this.sheetName = sheetName;
            this.methodParamSplitor = methodParamSplitor;
        }

        /// <summary>
        /// 是否为execFloat类型计算方法
        /// 只有默认和转义不是
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRelateMethod(CpmInfoMethodName? name) {
            if (name == null || name == CpmInfoMethodName.Default || name == CpmInfoMethodName.Escape) {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 加载采集参数配置
        /// </summary>
        /// <returns></returns>
        public List<CpmInfo> Load() {
            List<CpmInfo> cpmInfos = new List<CpmInfo>();
            using (var xlsOp = new XlsService(xlsPath)) {
                DataTable dt = xlsOp.ExcelToDataTable(sheetName, true);
                bool isStartRow = false;
                foreach (DataRow row in dt.Rows) {
                    if (row[0].ToString().ToUpper() == "START") {
                        isStartRow = true;
                        continue;
                    }
                    if (!isStartRow) {
                        continue;
                    }
                    CpmInfo cpmInfo = new CpmInfo();

                    var cpmStr = CpmLoaderOp.Load(row);
                    if (string.IsNullOrEmpty(cpmStr.Name)) {
                        continue;
                    }
                    //名称
                    cpmInfo.Name = cpmStr.Name;
                    //编码
                    cpmInfo.Code = int.Parse(cpmStr.Code.Trim());
                    if (DefinedParamCode.IsRfidParam(cpmInfo.Code)) {
                        continue;
                    }
                    //单位
                    cpmInfo.Unit = cpmStr.Unit;
                    //算法
                    cpmInfo.MethodName = string.IsNullOrEmpty(cpmStr.MethodName) ? default(CpmInfoMethodName?) : (CpmInfoMethodName)int.Parse(cpmStr.MethodName);
                    //算法参数（字符串）
                    cpmInfo.MethodParamStrs = string.IsNullOrEmpty(cpmStr.MethodParams) ? null : cpmStr.MethodParams.Split(new string[] { methodParamSplitor }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //报警的Bom内容的Key
                    if (!string.IsNullOrEmpty(cpmStr.MqAlarm)) {
                        cpmInfo.MqAlarmBomKeys = cpmStr.MqAlarm.Trim().Split('|');
                    }
                    if (!string.IsNullOrEmpty(cpmStr.ExpAlarm)) {
                        cpmInfo.ExpAlarms = cpmStr.ExpAlarm?.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    cpmInfo.PlcAlarmKey = cpmStr.PlcAlarm;
                    //算法参数（计算型）
                    if (IsRelateMethod(cpmInfo.MethodName)) {
                        cpmInfo.MethodParamInts = new List<int>(cpmInfo.MethodParamStrs.Count);
                        cpmInfo.MethodParamStrs.ForEach(str => {
                            cpmInfo.MethodParamInts.Add(int.Parse(str));
                        });
                    }

                    cpmInfos.Add(cpmInfo);
                }
            }
            return cpmInfos;
        }
    }


    /// <summary>
    /// 封装dt到cpm的转换
    /// </summary>
    public static class CpmLoaderOp {
        public static CpmStr Load(DataRow row) {
            var cpmStr = new CpmStr() {
                Name = row["名称"].ToString(),
                Code = row["编码"].ToString(),
                Unit = row["单位"].ToString(),
                MethodName = row["算法"].ToString(),
                MethodParams = row["算法参数"].ToString(),
            };

            cpmStr.PlcAlarm = row.GetValue("Plc报警配置");
            //兼容老版本配置
            cpmStr.MqAlarm = row.GetValue("报警配置");
            if (string.IsNullOrEmpty(cpmStr.MqAlarm)) {
                cpmStr.MqAlarm = row.GetValue("Mq报警配置");
            }
            cpmStr.ExpAlarm = row.GetValue("Exp报警配置");

            return cpmStr;
        }

        /// <summary>
        /// 采集参数配置是一个dataTable，这是生成头部
        /// </summary>
        /// <returns></returns>
        public static DataTable CreateCpmHeadDt() {
            DataTable dt = new DataTable();
            dt.Columns.Add("名称");
            dt.Columns.Add("编码");
            dt.Columns.Add("单位");
            dt.Columns.Add("算法参数");
            dt.Columns.Add("算法");
            return dt;
        }
    }

    /// <summary>
    /// 在xls中采集参数都是string
    /// </summary>
    public class CpmStr {
        public string Code;
        public string Name;
        public string Unit;
        public string MethodName;
        public string MethodParams;
        public string MqAlarm;
        public string PlcAlarm;
        public string ExpAlarm;
    }

    /// <summary>
    /// Plc报警
    /// 含有参数编码、最大值参数编码、最小值参数编码
    /// <author>ychost</author>
    /// <date>2017-12-26</date>
    /// </summary>
    public class PlcAlarmCpm {
        /// <summary>
        /// 待报警参数编码
        /// </summary>
        public int Code;
        /// <summary>
        /// 最大值参数编码
        /// </summary>
        public int? MaxCode;
        /// <summary>
        /// 最小值参数编码
        /// </summary>
        public int? MinCode;
        /// <summary>
        /// 报警关键字
        /// </summary>
        public string AlarmKey;
    }

    /// <summary>
    /// 经验报警，固定的最大值和最小值
    /// <author>ychost</author>
    /// <date>2018-1-29</date>
    /// </summary>
    public class ExpAlarm {
        public float? Max;
        public float? Min;
    }
}
