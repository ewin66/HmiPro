using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Util {
    /// <summary>
    /// 算法部分
    /// <date>2017-10-04</date>
    /// <author>ychost</author>
    /// </summary>
    public static partial class YUtil {

        /// <summary>
        /// 计算平均值所需要的数据
        /// </summary>
        class YExecAvg {
            public double Xn;
            public int N_1;
            public double Xn_1_Avg;
        }
        public static Func<double, double> CreateExecAvgFunc() {
            var ap = new YExecAvg();
            return (xn) => {
                var n = ap.N_1 + 1;
                var xnAvg = ap.Xn_1_Avg + (1.0 / n) * (xn - ap.Xn_1_Avg);
                ap.N_1 = n;
                ap.Xn_1_Avg = xnAvg;
                return xnAvg;
            };
        }


        /// <summary>
        /// 计算标准差
        /// </summary>
        class YExecStdDev {
            public int N_1;
            public double Xn_1_Avg;
            public double Sn_1_Pow2;
        }
        public static Func<double, double> CreateExecStdDevFunc() {
            YExecStdDev ap = new YExecStdDev();
            return (xn) => {
                var n = ap.N_1 + 1;
                var snPow2 = ((double)ap.N_1 / n) * (ap.Sn_1_Pow2 + (1.0 / n) * Math.Pow(xn - ap.Xn_1_Avg, 2));
                var xnAvg = ap.Xn_1_Avg + ((double)1 / n) * (xn - ap.Xn_1_Avg);
                ap.N_1 = n;
                ap.Xn_1_Avg = xnAvg;
                ap.Sn_1_Pow2 = snPow2;
                return Math.Pow(snPow2, 0.5);
            };
        }

        /// <summary>
        /// 计算导数，横坐标的增量为1
        /// </summary>
        class YExecDer {
            public double LatestVal=0;
        }

        public static Func<double, double> CreateExecDerFunc() {
            YExecDer ap = new YExecDer();
            return (xn) => {
                var der = xn - ap.LatestVal;
                ap.LatestVal = xn;
                return der;
            };
        }

        /// <summary>
        /// 计算倒数，除数可以为0，结果为double.MaxValue
        /// </summary>
        /// <returns></returns>
        public static Func<double, double> CreateExecRcpFunc() {
            return (x) => {
                var ans = 0.0;
                try {
                    ans = 1 / x;
                }
                catch {
                    ans = double.MaxValue;
                }
                return ans;
            };
        }
    }
}
