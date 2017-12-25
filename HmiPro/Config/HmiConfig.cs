using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Config {
    /// <summary>
    /// 配置一些所有机台通用的属性，比如
    /// mq连接地址，监听端口等等
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class HmiConfig {

        public static void Load(string path) {
            var config = YUtil.GetJsonObjectFromFile<Dictionary<string, object>>(path);
            YUtil.SetStaticField(typeof(HmiConfig), config);
            YUtil.ValidRequiredConfig(typeof(HmiConfig));
            CraftBomZhsDict = new Dictionary<string, string>();

        }

        /// <summary>
        /// 用xls文件初始化排产任务中的工艺Bom汉化字典
        /// </summary>
        /// <param name="xlsPath"></param>
        public static void InitCraftBomZhsDict(string xlsPath) {
            using (var xlsOp = new XlsService(xlsPath)) {
                var dt = xlsOp.ExcelToDataTable("bom", true);
                foreach (DataRow row in dt.Rows) {
                    if (!string.IsNullOrEmpty(row["comment"].ToString())) {
                        CraftBomZhsDict[row["column"].ToString()] = row["comment"].ToString();
                    }
                }
            }
        }

        //启动界面
        [Required] public static readonly string StartView;

        [Required] public static readonly string WindowStyle;
        //更新地址
        [Required] public static readonly string UpdateUrl;

        [Required] public static readonly int CmdHttpPort;

        //采集参数监控的ip地址
        [Required] public static readonly string CpmTcpIp;

        //端口
        [Required] public static readonly int CpmTcpPort;

        //更新Web看板数据周期
        [Required] public static readonly int UploadWebBoardInterval;

        /// <summary>
        /// 上传属性数据
        /// </summary>
        [Required] public static readonly string QueWebSrvPropSave;

        /// <summary>
        /// 上传异常
        /// </summary>
        [Required] public static readonly string QueWebSrvException;

        /// <summary>
        /// 上传压缩的采集参数
        /// </summary>
        [Required] public static readonly string QueWebSrvZipParam;

        /// <summary>
        /// 呼叫叉车
        /// </summary>
        [Required] public static readonly string QueCallForklift;

        /// <summary>
        /// 实时更新电子看板
        /// </summary>
        [Required] public static readonly string QueUpdateWebBoard;

        /// <summary>
        /// 监听andorid的消息
        /// </summary>
        [Required] public static readonly string TopicListenHandSet;

        [Required] public static readonly string TopicEmpRfid;

        //上传电能
        [Required] public static readonly string QueUploadPowerElec;
        //采集参数超时，超过该时间未有采集参数则清空实时数据表
        [Required] public static readonly int CpmTimeout;


        [Required] public static readonly string QueUploadOee;

        /// <summary>
        /// mongodb连接
        /// </summary>
        [Required] public static readonly string MongoConn;
        /// <summary>
        /// 中间件连接
        /// </summary>
        [Required] public static readonly string MqConn;
        /// <summary>
        /// 中间件登录名
        /// </summary>
        [Required] public static readonly string MqUserName;
        /// <summary>
        /// 中间件登录密码
        /// </summary>
        [Required] public static readonly string MqUserPwd;
        /// <summary>
        /// 保留小数位数
        /// </summary>
        [Required]
        public static readonly int MathRound;
        /// <summary>
        /// 开发用户名字，可以是多个，用","隔开,主要是为了判断目前运行的环境是否为开发人员所有
        /// 这样便于动态区分环境
        /// </summary>
        [Required]
        public static readonly string DevUser;
        /// <summary>
        /// 是否为开发环境
        /// </summary>
        [Required]
        public static readonly bool IsDevEnv;
        /// <summary>
        /// 时间服务器ip，端口使用默认123
        /// </summary>
        [Required] public static readonly string NtpIp;
        //InfluxDb地址
        [Required] public static readonly string InfluxDbIp;
        //InfluxDb采集参数数据库名称
        [Required] public static readonly string InfluxCpmDbName;

        [Required] public static readonly int CloseScreenInterval;
        /// <summary>
        /// Bom表汉化字典
        /// </summary>
        public static IDictionary<string, string> CraftBomZhsDict;

        //== 来自外部命令行和约定配置
        public static string SqlitePath;
        public static readonly string LogFolder = @"C:\HmiPro\Log\";
    }

}
