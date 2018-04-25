using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Utils.Text.Internal;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Mocks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using HmiPro.ViewModels.DMes.Form;
using HmiPro.ViewModels.Sys.Form;
using YCsharp.Model.Procotol.SmParam;
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
        /// 同步时间
        /// </summary>
        [Command(Name = "SyncTimeCommand")]
        public void SyncTime() {
            Task.Run(() => {
                try {
                    //获取服务器时间
                    var ntpTime = YUtil.GetNtpTime(HmiConfig.NtpIp);
                    App.StartupLog.SyncServerTime = ntpTime;
                    //时间差超过10秒才同步时间
                    if (Math.Abs((DateTime.Now - ntpTime).TotalSeconds) > 10) {
                        YUtil.SetLoadTimeByDateTime(ntpTime);
                    }
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "通知",
                        Content = "同步时间成功"
                    }));
                } catch (Exception e) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "警告",
                        Content = "同步时间失败"
                    }));
                }
            });
        }

        /// <summary>
        /// 打开物理报警灯
        /// </summary>
        /// <param name="ms">响铃毫秒数</param>
        [Command(Name = "OpenAlarmCommand")]
        public void OpenAlarm(int ms) {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in MachineConfig.MachineDict) {
                App.Store.Dispatch(new AlarmActions.OpenAlarmLights(pair.Key, ms));
                builder.Append(pair.Key).Append(" ");
            }
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "执行完毕",
                Content = "已发送打开报警灯指令给 " + builder.ToString()
            }));
        }

        /// <summary>
        /// 关闭报警灯
        /// </summary>
        [Command(Name = "CloseAlarmCommand")]
        public void CloseAlarm() {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in MachineConfig.MachineDict) {
                builder.Append(pair.Key).Append(" ");
                App.Store.Dispatch(new AlarmActions.CloseAlarmLights(pair.Key));
            }
            App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                Title = "执行完毕",
                Content = "已发送关闭报警灯指令给 " + builder.ToString()
            }));
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

        [Command(Name = "ShowConfirmFormCommand")]
        public void ShowConfirmForm() {
            var frm = new CompleteAxisForm() {
                OnOkPressed = f => {
                    CompleteAxisForm cf = f as CompleteAxisForm;


                    var display = cf.CompleteStatus.GetAttribute<DisplayAttribute>();
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Content = "您点击了确认，且选择了 " + display?.Name
                    }));
                },
                OnCancelPressed = f => {
                    CompleteAxisForm cf = f as CompleteAxisForm;
                    var display = cf.CompleteStatus.GetAttribute<DisplayAttribute>();

                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Content = "您点击了取消，且选择了 " + display?.Name
                    }));
                }
            };
            App.Store.Dispatch(new SysActions.ShowFormView("确认完成该轴", frm));
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

        [Command(Name = "HideDesktopCommand")]
        public void HideDesktop() {
            YUtil.HideDesktop(AssetsHelper.GetAssets().ExeNirCmd);
        }

        [Command(Name = "ShowDesktopCommand")]
        public void ShowDesktop() {
            YUtil.ShowDesktop(AssetsHelper.GetAssets().ExeNirCmd);
        }

        [Command(Name = "ShowElecPowerCommand")]
        public void ShowElecPower() {
            foreach (var pair in MachineConfig.MachineDict) {
                var elec = getElecPower(pair.Key);
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = pair.Key + " 总电能",
                    Content = elec.ToString("0.00"),
                    Level = NotifyLevel.Info
                }));
            }
        }

        float getElecPower(String machineCode) {
            var cpmNameToCodeDict = MachineConfig.MachineDict[machineCode].CpmNameToCodeDict;
            var setting = GlobalConfig.MachineSettingDict[machineCode];
            //update:2018-4-13，添加总电能
            if (cpmNameToCodeDict.ContainsKey(setting.totalPower)) {
                if (App.Store.GetState().CpmState.OnlineCpmsDict[machineCode]
                    .TryGetValue(cpmNameToCodeDict[setting.totalPower], out var tp)) {
                    if (tp.ValueType == SmParamType.Signal) {
                        return tp.GetFloatVal();
                    }
                }
            }
            return 0;
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