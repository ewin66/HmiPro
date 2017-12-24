using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Mocks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.ViewModels.Sys {
    [POCOViewModel]
    public class TestViewModel {
        public TestViewModel() {
        }

        private Func<int> rand = YUtil.GetRandomIntGen(0, 10);

        [Command(Name = "OpenAlarmCommand")]
        public void OpenAlarm(int ms) {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            //App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(machineCode, AlarmMocks.CreateOneAlarm(rand())));
            App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, ms));

        }


        [Command(Name = "CloseAlarmCommand")]
        public void CloseAlarm() {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            App.Store.Dispatch(new AlarmActions.CloseAlarmLights(machineCode));
        }

        [Command(Name = "CloseScreenCommand")]
        public void CloseScreen() {
            App.Store.Dispatch(new SysActions.CloseScreen());
        }

    }
}