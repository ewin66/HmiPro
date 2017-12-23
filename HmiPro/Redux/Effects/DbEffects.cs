using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using MongoDB.Driver;
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
        /// 上传报警数据到MongoDb
        /// </summary>
        public StorePro<AppState>.AsyncActionNeedsParam<DbActions.UploadAlarmsMongo> UploadAlarmsMongo;
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
            initUploadAlarmsMongo();
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
        /// 保存报警数据
        /// </summary>
        void initUploadAlarmsMongo() {
            UploadAlarmsMongo = App.Store.asyncActionVoid<DbActions.UploadAlarmsMongo>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    await MongoHelper.GetMongoService().GetDatabase(instance.MachineCode).GetCollection<MqAlarm>(instance.Collection).InsertOneAsync(instance.MqAlarm);
                    App.Store.Dispatch(new SimpleAction(DbActions.UPLOAD_ALARMS_MONGO_SUCCESS));
                });
        }

        /// <summary>
        /// 上传采集参数到时态数据库
        /// </summary>
        void initUploadCpmsInfluxDb() {
            UploadCpmsInfluxDb = App.Store.asyncActionVoid<DbActions.UploadCpmsInfluxDb>(
              async (dispatch, getState, instance) => {
                  await Task.Run(() => {
                      dispatch(instance);
                      bool success = InfluxDbHelper.GetInfluxDbService().WriteCpms(instance.MachineCode, instance.Cpms.ToArray());
                      if (success) {
                          App.Store.Dispatch(new DbActions.UploadCpmsInfluxDbSuccess());
                      } else {
                          App.Store.Dispatch(new DbActions.UploadCpmsInfluxDbFailed());
                      }
                  });
              });
        }
    }
}
