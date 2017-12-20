using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraPrinting.Native;
using HmiPro.Config;
using HmiPro.Config.Models;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-20</date>
    /// <author>ychost</author>
    /// </summary>
    public class OeeEffects {

        public readonly LoggerService Logger;
        public StorePro<AppState>.AsyncActionNeedsParam<OeeActions.StartCalcOeeTimer> StartCalcOeeTimer;
        private readonly OeeService oeeService;
        public OeeEffects(OeeService oeeService) {
            UnityIocService.AssertIsFirstInject(GetType());
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            this.oeeService = oeeService;
            initStartCalcOeeTimer();
        }

        void initStartCalcOeeTimer() {
            StartCalcOeeTimer = App.Store.asyncActionVoid<OeeActions.StartCalcOeeTimer>(
              async (dispatch, getState, intance) => {
                  dispatch(intance);
                  YUtil.SetInterval(intance.Interval, () => {
                      foreach (var pair in getState().CpmState.MachineStateDict) {
                          var machineCode = pair.Key;
                          if (!MachineConfig.MachineDict[machineCode].LogicToCpmDict.ContainsKey(CpmInfoLogic.Speed)) {
                              Logger.Error($"机台 {machineCode} 未配置速度逻辑，无法判断开停机，无法计算 Oee - 时间效率");
                              return;
                          }

                          var machineStates = pair.Value;
                          var currentSpeed = (float)getState().CpmState.SpeedDict[machineCode].Value;
                          var runTimeSec = oeeService.GetMachineRunTimeSec(machineStates, currentSpeed);
                          var debugTimeSec = oeeService.GetMachineDebugTimeSec();
                          var workTime = YUtil.GetKeystoneWorkTime();
                          //计算时间效率
                          if (runTimeSec < 0) {
                              Logger.Error($"计算时间效率失败，有效时间 {runTimeSec} < 0 ");
                          } else {
                              float timeEff = (float)((runTimeSec - debugTimeSec) / (DateTime.Now - workTime).TotalSeconds);
                              App.Store.Dispatch(new OeeActions.NotifyOeeCacled(machineCode, timeEff,null,null));
                          }
                      }
                  });
              });
        }
    }
}
