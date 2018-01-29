using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DevExpress.Mvvm;
using DevExpress.Xpf.WindowsUI;
using HmiPro.Redux.Models;
using HmiPro.ViewModels.Sys;
using HmiPro.Views.Sys;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 系统功能相关动作，配置都存在sqlite中的
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class SysActions {
        //设置界面，目前在 Prod 中已经不用了
        public static readonly string SHOW_SETTING_VIEW = "[Sys] Show Setting View";
        public static readonly string SHUTDOWN_APP = "[Sys] Shutdown App";

        //Http 系统相关指令
        public static readonly string START_HTTP_SYSTEM = "[Sys] Start Http System";
        public static readonly string START_HTTP_SYSTEM_SUCCESS = "[Sys] Start Http System Success";
        public static readonly string START_HTTP_SYSTEM_FAILED = "[Sys] Start Http System Failed";
        public static readonly string HTTP_SYSTEM_INVOKE = "[Sys] Http System Invoke";

        //程序更新状态
        public static readonly string FIND_UPDATED_VERSION = "[Sys] Find Updated Version";
        public static readonly string NOT_FIND_UPDATED_VERSION = "[Sys] Not Find Updated Version";

        //整个程序初始化完成，包括配置文件、Helper、定时器等等都初始化完成
        public static readonly string APP_INIT_COMPLETED = "[Sys] App Init Completed";

        //对显示器的操作
        public static readonly string CLOSE_SCREEN = "[Sys] Close Screen";
        public static readonly string OPEN_SCREEN = "[Sys] Open Screen";
        public static readonly string START_CLOSE_SCREEN_TIMER = "[Sys] Start Turn Off Screen Timer";
        public static readonly string STOP_CLOSE_SCREEN_TIMER = "[Sys] Stop Turn Off Screen Timer";

        //右上角提示
        public static readonly string SHOW_NOTIFICATION = "[Sys] Show Notification";

        //添加和删除跑马灯文字内容，注意：添加是 AddOrUpdate 操作
        public static readonly string ADD_MARQUEE_MESSAGE = "[Sys] Add Marquee Message";
        public static readonly string DEL_MARQUEE_MESSAGE = "[Sys] Delete Marquee Message";

        //弹出的Modal框（DataLayout布局）
        public static readonly string SHOW_FORM_VIEW = "[Sys] Show Form View";
        public static readonly string FORM_VIEW_PRESSED_OK = "[Sys] Form View Pressed Ok";
        public static readonly string FORM_VIEW_PRESSED_CANCEL = "[Sys] Form View Pressed Cancel";

        //跑马灯内容的一些 Id
        public static readonly string MARQUEE_SCAN_END_AXIS_RFID = "[Marquee Id] Scan End Axis Rfid";
        public static readonly string MARQUEE_SCAN_START_AXIS_RFID = "[Marquee Id] Scan Start Axis Rfid";
        public static readonly string MARQUEE_PUNCH_START_MACHINE = "[Marquee Id] Punch Start Machine";
        public static readonly string MARQUEE_PING_IP_FAILED = "[Marquee Id] Ping Ip Failed";
        public static readonly string MARQUEE_APP_START_TIMEOUT = "[Marquee Id] App Start Timeout";
        public static readonly string MARQUEE_LOG_FOLDER_TOO_LARGE = "[Marquee Id] Log Folder Too Large";

        //Loading 界面
        public static readonly string SET_LOADING_MESSAGE = "[Sys] Set Loading Message";

        //app.xaml.cs初始化完毕
        public static readonly string APP_XAML_INITED = "[Sys] App Xaml Inited";

        //重启软件
        public static readonly string RESTART_APP = "[Sys] Restart App";

        //桌面操作
        public static readonly string HIDE_DESKTOP = "[Sys] Hide Desktop";
        public static readonly string SHOW_DESKTOP = "[Sys] Show Desktop";
        public static readonly string RETURN_DESKTOP = "[Sys] Return Desktop";

        //任务栏操作
        public static readonly string HIDE_TASK_BAR = "[Sys] Hide Task Bar";
        public static readonly string SHOW_TASK_BAR = "[Sys] Show Task Bar";

        //关闭 「正在加载中..」的 Loading 框框
        //某些 Hmi 无法自动关闭，所以在 OnLoad 里面手动关闭
        public static readonly string CLOSE_LOADING_SPLASH = "[Sys] Close Loading Splash";

        //更换壁纸
        public static readonly string CHANGE_WINDOW_BACKGROUND_IMAGE = "[Sys] Change Window Background Image";

        //使背景模糊
        public static readonly string MAKE_WINDOW_BACKGROUND_BLUR = "[Sys] Make Window Background Blur";

        public struct MakeWindowBackgroundBlur : IAction {
            public string Type() => MAKE_WINDOW_BACKGROUND_BLUR;

        }

        public struct ChangeWindowBackgroundImage : IAction {
            public string Type() => CHANGE_WINDOW_BACKGROUND_IMAGE;
            public string ImagePath;

            public ChangeWindowBackgroundImage(string imagePath) {
                ImagePath = imagePath;
            }
        }

        public struct CloseLoadingSplash : IAction {
            public string Type() => CLOSE_LOADING_SPLASH;
        }


        public struct HideDesktop : IAction {
            public string Type() => HIDE_DESKTOP;
        }

        public struct ShowDesktop : IAction {
            public string Type() => SHOW_DESKTOP;
        }

        public struct ReturnDesktop : IAction {
            public string Type() => RETURN_DESKTOP;
        }

        public struct HideTaskBar : IAction {
            public string Type() => HIDE_TASK_BAR;
        }

        public struct ShowTaskBar : IAction {
            public string Type() => SHOW_TASK_BAR;
        }


        public struct RestartApp : IAction {
            public string Type() => RESTART_APP;
            /// <summary>
            /// 程序延迟启动多少秒
            /// </summary>
            public int WaitSec;

            public RestartApp(int waitSec) {
                WaitSec = waitSec;
            }
        }


        public struct SetLoadingMessage : IAction {
            public string Type() => SET_LOADING_MESSAGE;
            public string Message;
            public double Percent;

            public SetLoadingMessage(string message, double percent) {
                Message = message;
                Percent = percent;
            }
        }

        public struct AppXamlInited : IAction {
            public StartupEventArgs StartArgs;
            public string Type() => APP_XAML_INITED;

            public AppXamlInited(StartupEventArgs args) {
                StartArgs = args;
            }
        }

        public struct AddMarqueeMessage : IAction {
            public string Type() => ADD_MARQUEE_MESSAGE;
            public string Message;
            public string Id;

            public AddMarqueeMessage(string id, string message) {
                Message = message;
                Id = id;
            }
        }

        public struct DelMarqueeMessage : IAction {
            public string Type() => DEL_MARQUEE_MESSAGE;
            public string Id;

            public DelMarqueeMessage(string id) {
                Id = id;
            }
        }

        public struct FormViewPressedCancel : IAction {
            public string Type() => FORM_VIEW_PRESSED_CANCEL;
            public object FormCtrls;
            public string Title;

            public FormViewPressedCancel(string title, object formCtrls) {
                Title = title;
                FormCtrls = formCtrls;
            }
        }


        public struct FormViewPressedOk : IAction {
            public string Type() => FORM_VIEW_PRESSED_OK;
            public object Form;
            public string Title;


            public FormViewPressedOk(string title, object form) {
                Title = title;
                Form = form;
            }
        }

        public struct ShowFormView : IAction {
            public string Type() => SHOW_FORM_VIEW;
            public BaseForm Form;
            public string Title;

            public ShowFormView(string title, BaseForm form) {
                Title = title;
                Form = form;
            }
        }


        public struct ShowNotification : IAction {
            public string Type() => SHOW_NOTIFICATION;
            public SysNotificationMsg Message;

            public ShowNotification(SysNotificationMsg msg) {
                Message = msg;
            }
        }


        public struct StartCloseScreenTimer : IAction {
            public string Type() => START_CLOSE_SCREEN_TIMER;
            public double Interval;

            public StartCloseScreenTimer(double interval) {
                Interval = interval;

            }
        }

        public struct StopCloseScreenTimer : IAction {
            public string Type() => STOP_CLOSE_SCREEN_TIMER;
        }

        public struct ShowSettingView : IAction {
            public string Type() => SHOW_SETTING_VIEW;
        }

        public struct ShutdownApp : IAction {
            public string Type() => SHUTDOWN_APP;
        }

        public struct StartHttpSystem : IAction {
            public string Type() {
                return START_HTTP_SYSTEM;
            }

            public StartHttpSystem(string url) {
                Url = url;
            }

            public string Url;
        }

        public struct StartHttpSystemSuccess : IAction {
            public string Type() => START_HTTP_SYSTEM_SUCCESS;
        }

        public struct StartHttpSystemFailed : IAction {
            public string Type() => START_HTTP_SYSTEM_FAILED;
            public Exception e;
        }

        public struct FindUpdatedVersion : IAction {
            public string Type() => FIND_UPDATED_VERSION;
        }

        public struct NotFindUpdatedVersion : IAction {
            public string Type() => NOT_FIND_UPDATED_VERSION;
        }

        public struct HttpSystemInvoke : IAction {
            public string Type() => HTTP_SYSTEM_INVOKE;
            public string Cmd;
        }

        public struct AppInitCompleted : IAction {
            public string Type() => APP_INIT_COMPLETED;
        }

        public struct OpenScreen : IAction {
            public string Type() => OPEN_SCREEN;
        }
        public struct CloseScreen : IAction {
            public string Type() => CLOSE_SCREEN;
        }
    }
}
