using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Redux.Services {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class MqService {
        public readonly LoggerService Logger;
        public MqService() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }
        /// <summary>
        /// 处理排产任务
        /// </summary>
        /// <param name="json"></param>
        public void SchTaskAccept(string json) {
            MqSchTask schTask = null;
            try {
                schTask = JsonConvert.DeserializeObject<MqSchTask>(json);
            } catch (Exception e) {
                Logger.Warn("反序列化排产任务异常，json数据为：" + json);
            }
            if (schTask == null) {
                return;
            }
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                ctx.SavePersist(new Persist(schTask.maccode, json));
            }

            App.Store.Dispatch(new MqActiions.SchTaskAccept(schTask));
        }

        public void ScanMaterialAccept(string json) {

        }
    }
}
