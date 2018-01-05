using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;
using YCsharp.Model.Procotol.SmParam;
using YCsharp.Util;

namespace HmiPro.Helpers {
    /// <summary>
    /// InfluxService 创建辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class InfluxDbHelper {
        private static string addr;
        private static string dbName;

        public static void Init(string addr, string dbName) {
            InfluxDbHelper.addr = addr;
            InfluxDbHelper.dbName = dbName;
            influxDbService = new InfluxDbService(addr, dbName);
        }

        private static InfluxDbService influxDbService;

        public static InfluxDbService GetInfluxDbService() {
            if (influxDbService == null) {
                throw new Exception("请先初始化 InfluxDbHelper.Init(addr,dbName)");
            }
            return influxDbService;
        }


    }


    /// <summary>
    /// 推送数据到 InfluxDb 
    /// <author>ychost</author>
    /// <date>2017-11-07</date>
    /// </summary>
    public class InfluxDbService {
        public readonly string DbAddr;
        public readonly string DbName;

        public InfluxDbService(string dbAddr, string dbName) {
            DbAddr = dbAddr;
            DbName = dbName;
        }

        /// <summary>
        /// 往influxDb中写入数据
        /// </summary>
        /// <param name="tags">类似：param=hello,code=112211</param>
        /// <param name="val">数据值</param>
        /// <param name="measurement">表名</param>
        /// <returns></returns>
        public byte[] Write(string measurement, string tags, object val, DateTime? dateTime = null) {
            var postData = $"{measurement},{tags} value={val} ";
            if (dateTime.HasValue) {
                //转换成纳秒
                //fixed:不能插入时间
                postData += YUtil.GetUtcTimestampMs(dateTime.Value) + "000000";
            }
            postData = postData.Replace("/", "");
            var httpUri = $"{DbAddr}/write?db={DbName}";
            try {
                using (var webClient = new WebClient()) {
                    var result = webClient.UploadData(httpUri, Encoding.UTF8.GetBytes(postData));
                    return result;
                }
            } catch (WebException ex) {
                return Encoding.UTF8.GetBytes(ex.ToString());
            }
        }

        /// <summary>
        /// 写批量数据
        /// </summary>
        /// <param name="data">measurement,tag value=val [timestampNs]</param>
        /// <returns></returns>
        public byte[] WriteMulti(params string[] data) {
            var postData = string.Join("\n", data);
            //fixed：字符异常
            postData = postData.Replace("/", "");
            var httpUri = $"{DbAddr}/write?db={DbName}";
            try {
                using (var webClient = new WebClient()) {
                    var result = webClient.UploadData(httpUri, Encoding.UTF8.GetBytes(postData));
                    return result;
                }
            } catch (WebException ex) {
                return Encoding.UTF8.GetBytes(ex.ToString());
            }
        }

        /// <summary>
        /// 写入大量的采集参数到influxDb
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        public bool WriteCpms(string measurement, params Cpm[] cpms) {
            List<string> paramList = new List<string>();
            foreach (var cpm in cpms) {
                if (cpm.ValueType != SmParamType.Signal) {
                    continue;
                }
                var timeStamp = "";
                //fixed:不能插入时间
                timeStamp = YUtil.GetUtcTimestampMs(cpm.PickTime) + "000000";
                var param = $"{measurement},param={cpm.Name} value={cpm.Value} {timeStamp}";
                paramList.Add(param);
            }
            var resp = WriteMulti(paramList.ToArray());
            if (resp.Length > 0) {
                Console.WriteLine(Encoding.UTF8.GetString(resp));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 写入大量的采集参数到influxDb
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        public bool WriteCpms(string measurement, Cpm[] cpms, int offset, int count) {
            if (offset + count >= cpms.Length) {
                return false;
            }
            List<string> paramList = new List<string>();
            for (int i = offset; i < count + offset; i++) {
                var cpm = cpms[i];
                if (cpm.ValueType != SmParamType.Signal) {
                    continue;
                }
                var timeStamp = "";
                //fixed:不能插入时间
                timeStamp = YUtil.GetUtcTimestampMs(cpm.PickTime) + "000000";
                var param = $"{measurement},param={cpm.Name} value={cpm.Value} {timeStamp}";
                paramList.Add(param);
            }
            var resp = WriteMulti(paramList.ToArray());
            if (resp.Length > 0) {
                Console.WriteLine(Encoding.UTF8.GetString(resp));
                return false;
            }
            return true;
        }

    }
}
