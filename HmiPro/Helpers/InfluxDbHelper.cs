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
        /// <summary>
        /// 全局唯一的 InfluxDbService
        /// </summary>
        private static InfluxDbService influxDbService;
        /// <summary>
        /// 初始化 InfluxDb 
        /// </summary>
        /// <param name="addr">InfluxDb 地址</param>
        /// <param name="dbName">InfluxDb 数据库</param>
        public static void Init(string addr, string dbName) {
            influxDbService = new InfluxDbService(addr, dbName);
        }

        /// <summary>
        /// 获取全局的InfluxService
        /// 因为使用的是 Http 协议所以不用管理连接池
        /// </summary>
        /// <returns></returns>
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
            postData = postData.Replace("/", string.Empty).Replace("\\", string.Empty);
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
            var postData = new StringBuilder(string.Join("\n", data));
            //fixed：字符异常
            postData = postData.Replace("/", string.Empty).Replace("\\", string.Empty);
            var httpUri = $"{DbAddr}/write?db=cpm";
            try {
                using (var webClient = new WebClient()) {
                    var result = webClient.UploadData(httpUri, Encoding.UTF8.GetBytes(postData.ToString()));
                    postData.Length = 0;
                    return result;
                }
            } catch (WebException ex) {
                return Encoding.UTF8.GetBytes(ex.ToString());
            }
        }

        /// <summary>
        /// 一次性写入大量的行数据
        /// </summary>
        /// <param name="builders"></param>
        /// <returns></returns>
        public bool WriteMultiString(params StringBuilder[] builders) {
            StringBuilder writeBuilder = new StringBuilder();
            foreach (var builder in builders) {
                if (builder.Length > 0) {
                    writeBuilder.Append(builder.ToString() + "\n");
                }
            }
            if (writeBuilder.Length == 0) {
                return true;
            }
            writeBuilder.Remove(writeBuilder.Length - 1, 1);
            var httpUri = $"{DbAddr}/write?db={DbName}";
            try {
                using (var webClient = new WebClient()) {
                    webClient.UploadData(httpUri, Encoding.UTF8.GetBytes(writeBuilder.ToString()));
                    return true;
                }
            } catch (WebException ex) {
                Console.WriteLine(ex.Message);

            } finally {
                //清空内存
                writeBuilder.Length = 0;
                foreach (var builder in builders) {
                    builder.Length = 0;
                }

            }
            return false;
        }


        /// <summary>
        /// 写入大量的采集参数到influxDb
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        [Obsolete("请使用 WriteCpms2")]
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
        /// 获取写入 InfluxDb 的字符串
        /// 使用 StringBuilder 为了防止内存溢出
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        /// <param name="pickTime"></param>
        /// <returns></returns>
        public StringBuilder GetCpms2WriteString(string measurement, List<Cpm> cpms, DateTime pickTime) {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{measurement},tag=采集参数 ");
            var timestamp = YUtil.GetUtcTimestampMs(pickTime) + "000000";
            bool valid = false;
            foreach (var cpm in cpms) {
                if (cpm.ValueType != SmParamType.Signal) {
                    continue;
                }
                //fix: 由于名字空格不能插入的情况
                builder.Append($"{cpm.Name.Replace(" ", "")}={cpm.Value},");
                valid = true;
            }
            //无有效参数的时候返回空的 sb
            if (valid == false) {
                builder.Clear();
                return builder;
            }

            builder.Remove(builder.Length - 1, 1);
            builder.Append($" {timestamp}");
            builder = builder.Replace("/", string.Empty).Replace("\\", string.Empty);
            return builder;
        }

        /// <summary>
        /// 2018-3-25 更新，通过将一个机台的所有参数放入同一 tag 的 不同 value
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        /// <param name="pickTime"></param>
        /// <returns></returns>
        public bool WriteCpms2(string measurement, List<Cpm> cpms, DateTime pickTime) {
            StringBuilder builder = GetCpms2WriteString(measurement, cpms, pickTime);
            var httpUri = $"{DbAddr}/write?db={DbName}";
            try {
                using (var webClient = new WebClient()) {
                    webClient.UploadData(httpUri, Encoding.UTF8.GetBytes(builder.ToString()));
                    return true;
                }
            } catch {
            }
            return false;
        }

        /// <summary>
        /// 写入大量的采集参数到influxDb
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="cpms"></param>
        [Obsolete("请使用 WriteCpms2")]
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
                var param = $"{measurement},param={cpm.Name.Replace(" ","")} value={cpm.Value} {timeStamp}";
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
