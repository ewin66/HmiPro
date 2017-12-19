using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-19</date>
    /// </summary>
    public class DbEffects {
        public StorePro<AppState>.AsyncActionNeedsParam<DbActions.UploadCpmsInfluxDb> UploadCpmsInfluxDb;

        public DbEffects() {
            UnityIocService.AssertIsFirstInject(GetType());
            initUploadCpmsInfluxDb();
        }

        /// <summary>
        /// 上传采集参数到时态数据库
        /// </summary>
        void initUploadCpmsInfluxDb() {
            UploadCpmsInfluxDb = App.Store.asyncActionVoid<DbActions.UploadCpmsInfluxDb>(
              async (dispatch, getState, instance) => {
                  await Task.Run(() => {
                      dispatch(instance);
                      InfluxDbHelper.GetInfluxDbService().WriteCpms(instance.MachineCode, instance.Cpms.ToArray());
                  });
              });
        }
    }
}
