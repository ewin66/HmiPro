using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Mocks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels.Sys {
    [POCOViewModel]
    public class TestViewModel {
        public TestViewModel() {
        }

        private Func<int> rand = YUtil.GetRandomIntGen(0, 10);

        [Command(Name = "OnLoadedCommand")]
        public void Onloaded() {
            App.Store.Dispatch(new SysActions.CloseLoadingSplash());
        }

        [Command(Name = "OpenAlarmCommand")]
        public void OpenAlarm(int ms) {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, ms));

        }

        [Command(Name = "CloseAlarmCommand")]
        public void CloseAlarm() {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            App.Store.Dispatch(new AlarmActions.CloseAlarmLights(machineCode));
        }

        [Command(Name = "CloseScreenCommand")]
        public void CloseScreen(object secObj) {
            if (secObj == null) {
                YUtil.CloseScreenByNirCmd(AssetsHelper.GetAssets().ExeNirCmd);
            } else {
                int sec = int.Parse(secObj.ToString());
                var ms = sec * 1000;
                Task.Run(() => {
                    YUtil.CloseScreenByNirCmd(AssetsHelper.GetAssets().ExeNirCmd);
                    YUtil.SetTimeout(ms, () => {
                        YUtil.OpenScreenByNirCmmd(AssetsHelper.GetAssets().ExeNirCmd);
                    });
                });
            }
        }


        [Command(Name = "CheckUpdateCommand")]
        public void CheckUpdate() {
            var sysService = UnityIocService.ResolveDepend<SysService>();
            Task.Run(() => {
                if (sysService.CheckUpdate()) {
                    sysService.StartUpdate();
                } else {
                    App.Store.Dispatch(
                        new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "未检查到更新",
                            Content = "本软件版本目前已是最新版"
                        }));
                }
            });
        }

        [Command(Name = "OpenScreenCommand")]
        public void OpenScreen() {
            YUtil.OpenScreenByNirCmmd(AssetsHelper.GetAssets().ExeNirCmd);
        }

        [Command(Name = "StandbyScreenCommand")]
        public void StandbyScreen() {
        }

        [Command(Name = "ShowNotificationCommand")]
        public void ShowNotification() {
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "测试通知",
                Content = "测试通知信息成功",
            }));
        }

        [Command(Name = "OskCommand")]
        public void ShowKeyboardCommand() {
            YUtil.CallOskAsync();
        }

        [Command(Name = "CloseAppCommand")]
        public void CloseApp() {
            App.Store.Dispatch(new SysActions.ShutdownApp());
        }

        /// <summary>
        /// 显示桌面
        /// </summary>
        [Command(Name = "ReturnDesktopCommand")]
        public void ReturnDesktop() {
            //返回桌面之前应显示任务栏，不然没办法找到软件
            App.Store.Dispatch(new SysActions.ShowTaskBar());
            App.Store.Dispatch(new SysActions.ReturnDesktop());
        }

        [Command(Name = "HideTaskBarCommand")]
        public void HideTaskBar() {
            App.Store.Dispatch(new SysActions.HideTaskBar());
        }

        [Command(Name = "ShowTaskBarCommand")]
        public void ShowTaskBar() {
            App.Store.Dispatch(new SysActions.ShowTaskBar());
        }

        [Command(Name = "ShowTaskMgmrCommand")]
        public void ShowTaskMgmr() {
            YUtil.CallTaskMgrAsync();
        }

        [Command(Name = "ShowLogFolerCommand")]
        public void ShowLogFoler() {
            Task.Run(() => {
                YUtil.Exec(HmiConfig.LogFolder, "");
            });
        }

        [Command(Name = "CloseLoadingSplashCommand1")]
        public void CloseLoadingSplash1() {
            App.Store.Dispatch(new SysActions.CloseLoadingSplash());
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "通知",
                Content = "完成 Splash 关闭 动作"
            }));
        }

        [Command(Name = "CloseLoadingSplashCommand2")]
        public void CloseLoadingSplash2() {
            try {
                DXSplashScreen.Close();
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "通知",
                    Content = "关闭 Splash 成功"
                }));
            } catch {
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "警告",
                    Content = "关闭 Splash 失败"
                }));
            }
        }

        [Command(Name = "ClosePcCommand")]
        public void ClosePc() {
            YUtil.ShutDownPc();
        }
    }
}