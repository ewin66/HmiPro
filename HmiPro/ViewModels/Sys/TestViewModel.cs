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
using HmiPro.ViewModels.Sys.Form;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.ViewModels.Sys {
    [POCOViewModel]
    public class TestViewModel {
        public TestViewModel() {
        }

        /// <summary>
        /// 惯例，关闭 Loading 
        /// </summary>
        [Command(Name = "OnLoadedCommand")]
        public void Onloaded() {
            App.Store.Dispatch(new SysActions.CloseLoadingSplash());
        }

        /// <summary>
        /// 打开物理报警灯
        /// </summary>
        /// <param name="ms">响铃毫秒数</param>
        [Command(Name = "OpenAlarmCommand")]
        public void OpenAlarm(int ms) {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            App.Store.Dispatch(new AlarmActions.OpenAlarmLights(machineCode, ms));

        }

        /// <summary>
        /// 关闭报警灯
        /// </summary>
        [Command(Name = "CloseAlarmCommand")]
        public void CloseAlarm() {
            var machineCode = MachineConfig.MachineDict.FirstOrDefault().Key;
            App.Store.Dispatch(new AlarmActions.CloseAlarmLights(machineCode));
        }

        /// <summary>
        /// 关闭显示器
        /// </summary>
        /// <param name="secObj"></param>
        [Command(Name = "CloseScreenCommand")]
        public void CloseScreen(object secObj) {
            if (secObj == null) {
                YUtil.CloseScreen(AssetsHelper.GetAssets().ExeNirCmd);
            } else {
                int sec = int.Parse(secObj.ToString());
                var ms = sec * 1000;
                Task.Run(() => {
                    YUtil.CloseScreen(AssetsHelper.GetAssets().ExeNirCmd);
                    YUtil.SetTimeout(ms, () => {
                        YUtil.OpenScreen(AssetsHelper.GetAssets().ExeNirCmd);
                    });
                });
            }
        }

        /// <summary>
        /// 打开显示器
        /// </summary>
        [Command(Name = "OpenScreenCommand")]
        public void OpenScreen() {
            YUtil.OpenScreen(AssetsHelper.GetAssets().ExeNirCmd);
        }

        /// <summary>
        /// 检查更新
        /// </summary>
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

        /// <summary>
        /// 显示系统的通知
        /// </summary>
        [Command(Name = "ShowNotificationCommand")]
        public void ShowNotification() {
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "测试通知",
                Content = "测试通知信息成功",
            }));
        }

        /// <summary>
        /// 弹出虚拟键盘
        /// </summary>
        [Command(Name = "OskCommand")]
        public void ShowKeyboardCommand() {
            ShowPasswordForm(YUtil.CallOskAsync);
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        [Command(Name = "CloseAppCommand")]
        public void CloseApp() {
            ShowPasswordForm(() => {
                App.Store.Dispatch(new SysActions.ShutdownApp());
            });
        }

        /// <summary>
        /// 显示桌面
        /// </summary>
        [Command(Name = "ReturnDesktopCommand")]
        public void ReturnDesktop() {
            ShowPasswordForm(() => {
                //返回桌面之前应显示任务栏，不然没办法找到软件
                App.Store.Dispatch(new SysActions.ShowTaskBar());
                App.Store.Dispatch(new SysActions.ReturnDesktop());
            });
        }

        /// <summary>
        /// 隐藏任务栏
        /// </summary>
        [Command(Name = "HideTaskBarCommand")]
        public void HideTaskBar() {
            ShowPasswordForm(() => {
                App.Store.Dispatch(new SysActions.HideTaskBar());
            });
        }

        /// <summary>
        /// 显示任务栏
        /// </summary>
        [Command(Name = "ShowTaskBarCommand")]
        public void ShowTaskBar() {
            ShowPasswordForm(() => {
                App.Store.Dispatch(new SysActions.ShowTaskBar());
            });
        }

        /// <summary>
        /// 弹出任务管理器
        /// </summary>
        [Command(Name = "ShowTaskMgmrCommand")]
        public void ShowTaskMgmr() {
            ShowPasswordForm(YUtil.CallTaskMgrAsync);
        }

        /// <summary>
        /// 显示日志文件夹
        /// </summary>
        [Command(Name = "ShowLogFolerCommand")]
        public void ShowLogFoler() {
            Task.Run(() => {
                ShowPasswordForm(() => {
                    YUtil.Exec(HmiConfig.LogFolder, "");
                });
            });
        }

        /// <summary>
        /// 关闭加载 Loading 窗体
        /// </summary>
        [Command(Name = "CloseLoadingSplashCommand1")]
        public void CloseLoadingSplash1() {
            App.Store.Dispatch(new SysActions.CloseLoadingSplash());
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "通知",
                Content = "完成 Splash 关闭 动作"
            }));
        }

        /// <summary>
        /// 另一种方式关闭  Loading 窗体
        /// </summary>
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

        /// <summary>
        /// 关闭电脑
        /// </summary>
        [Command(Name = "ClosePcCommand")]
        public void ClosePc() {
            ShowPasswordForm(YUtil.ShutdownPc);
        }

        /// <summary>
        /// 谨慎操作，删除程序！！
        /// </summary>
        [Command(Name = "DeleteAppCommand")]
        public void DeleteApp() {
            ShowPasswordForm(() => {
                App.Store.Dispatch(new HookActions.DangerDamageApp("莫生气"));
            }, "请输入密码，该操作相当危险", "123456");
        }
        /// <summary>
        /// 显示密码输入框的
        /// </summary>
        /// <param name="pwdValidAction"></param>
        /// <param name="title"></param>
        /// <param name="password"></param>
        public void ShowPasswordForm(Action pwdValidAction, string title = "请输入密码", string password = "0000") {
            if (!HmiConfig.UsePwdToAdmin) {
                pwdValidAction?.Invoke();
            } else {
                App.Store.Dispatch(new SysActions.ShowFormView(title, new PasswordForm() {
                    OnOkPressed = form => {
                        var pForm = (PasswordForm)form;
                        if (pForm.Password == password) {
                            pwdValidAction?.Invoke();
                        } else {
                            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                                Title = "警告",
                                Content = "密码错误"
                            }));
                        }
                    },
                }));
            }
        }
    }
}