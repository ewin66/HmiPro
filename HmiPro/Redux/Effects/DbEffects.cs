using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using MongoDB.Driver;
using NeoSmart.AsyncLock;
using YCsharp.Model.Buffers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class DbEffects {
        /// <summary>
        /// 上传实时采集采集参数到InfluxDB
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<DbActions.UploadCpmsInfluxDb> UploadCpmsInfluxDb;
        /// <summary>
        /// 上传普通的文档数据到MongoDb
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<DbActions.UploadDocToMongo> UploadDocToMongo;
        /// <summary>
        /// 上传批量的文档数据到MongoDb
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<DbActions.UploadDocManyToMongo> UploadDocManyToMongo;
        /// <summary>
        /// MongoDeriver 自带的服务
        /// 全局一个实例就行，不用Dispose
        /// </summary>
        public readonly MongoClient MongoService;

        public DbEffects() {
            UnityIocService.AssertIsFirstInject(GetType());
            MongoService = MongoHelper.GetMongoService();
            initUploadCpmsInfluxDb();
            initUploadDocMongo();
            initUploadDocManyMongo();
        }

        /// <summary>
        /// 上传批量的MongoDoc数据
        /// </summary>
        void initUploadDocManyMongo() {
            UploadDocManyToMongo = App.Store.asyncActionVoid<DbActions.UploadDocManyToMongo>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    await MongoService.GetDatabase(instance.DbName).GetCollection<MongoDoc>(instance.Collection)
                        .InsertManyAsync(instance.Docs);
                    App.Store.Dispatch(new SimpleAction(DbActions.UPLOAD_DOC_MANY_TO_MONGO_SUCCESS));
                });
        }


        /// <summary>
        /// 上传任何基于MongoDoc的数据
        /// </summary>
        void initUploadDocMongo() {
            UploadDocToMongo = App.Store.asyncActionVoid<DbActions.UploadDocToMongo>(async (dispatch, getState, instance) => {
                dispatch(instance);
                await MongoService.GetDatabase(instance.DbName).GetCollection<MongoDoc>(instance.Collection)
                     .InsertOneAsync(instance.Doc);
                App.Store.Dispatch(new SimpleAction(DbActions.UPLOAD_DOC_TO_MONGO_SUCCESS));
            });
        }

        /// <summary>
        /// cpm 缓存字典
        /// </summary>
        private static readonly IDictionary<string, CpmCache> cpmCacheDict = new Dictionary<string, CpmCache>();

        /// <summary>
        /// 上传采集参数到时态数据库
        /// </summary>
        void initUploadCpmsInfluxDb() {
            UploadCpmsInfluxDb = App.Store.asyncActionVoid<DbActions.UploadCpmsInfluxDb>(
              async (dispatch, getState, instance) => {
                  dispatch(instance);
                  if (!cpmCacheDict.ContainsKey(instance.MachineCode)) {
                      cpmCacheDict[instance.MachineCode] = new CpmCache(1024);
                  }
                  var cache = cpmCacheDict[instance.MachineCode];
                  //缓存
                  foreach (var cpm in instance.Cpms) {
                      cache.Add(cpm);
                  }
                  //上传周期同与mq保持一致
                  if ((DateTime.Now - cache.LastUploadTime).TotalMilliseconds > HmiConfig.UploadWebBoardInterval || cache.DataCount > 900) {
                      await Task.Run(() => {
                          var upCaches = cache.Caches;
                          var upCount = cache.DataCount;
                          cache.Clear();
                          cache.LastUploadTime = DateTime.Now;
                          bool success = InfluxDbHelper.GetInfluxDbService().WriteCpms(instance.MachineCode, upCaches, 0, upCount);
                          if (success) {
                              App.Store.Dispatch(new DbActions.UploadCpmsInfluxDbSuccess());
                          } else {
                              App.Store.Dispatch(new DbActions.UploadCpmsInfluxDbFailed());
                          }
                      });
                  }
              });
        }
    }

    public class CpmCache {
        public Cpm[] Caches;
        public int DataCount = 0;
        public DateTime LastUploadTime;

        public CpmCache(int size) {
            Caches = new Cpm[size];
        }

        public void Add(Cpm cpm) {
            lock (this) {
                //超限
                if (DataCount >= Caches.Length) {
                    DataCount = Caches.Length - 1;
                }
                Caches[DataCount++] = cpm;
            }
        }

        public void Clear() {
            lock (this) {
                DataCount = 0;
            }
        }
    }
}
