using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using MongoDB.Driver;
using YCsharp.Service;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 数据库相关操作
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class DbCore {
        public readonly LoggerService Logger;
        private readonly IDictionary<string, Action<AppState, IAction>> actionsExecDict = new Dictionary<string, Action<AppState, IAction>>();
        private bool assertInitOnce = true;
        public MongoClient MongoService;
        public DbCore() {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            MongoService = MongoHelper.GetMongoService();

        }

        public void Init() {
            if (!assertInitOnce) {
                throw new Exception("请勿重复调用 DbCore.Init");
            }
            assertInitOnce = false;

            
        }

        /// <summary>
        /// 往Mongo里面写入数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        private void doWriteToMongo(AppState state, IAction action) {
            var dbAction = (DbActions.UploadDocToMongo)action;
            MongoService.GetDatabase(dbAction.DbName).GetCollection<MongoDoc>(dbAction.Collection).InsertOneAsync(dbAction.Doc);
        }
    }
}
