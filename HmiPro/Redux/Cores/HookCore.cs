using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Cores {
    /// <summary>
    /// 程序暴露的「钩子」核心逻辑
    /// <author>Hack</author>
    /// <date>2018-2-1</date>
    /// </summary>
    public class HookCore {
        /// <summary>
        /// 日志
        /// </summary>
        public LoggerService Logger;
        /// <summary>
        /// 事件处理器
        /// </summary>
        IDictionary<string, Action<AppState, IAction>> hookExecutors;
        /// <summary>
        /// 只能注入一次
        /// </summary>
        public HookCore() {
            UnityIocService.AssertIsFirstInject(GetType());
        }

        /// <summary>
        /// 请在配置文件初始化后调用
        /// </summary>
        public void Init() {
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
            hookExecutors = new Dictionary<string, Action<AppState, IAction>>();
            hookExecutors[HookActions.HACK_APP_SKULL_VIEW] = hackSkullView;
            hookExecutors[HookActions.RESCUE_APP_SKULL_VIEW] = rescueSkullView;
            hookExecutors[HookActions.DANGER_DAMAGE_APP] = dangerDamageApp;
            App.Store.Subscribe(hookExecutors);
        }

        /// <summary>
        /// 显示骷颅头界面
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">骷髅头图片和内容</param>
        void hackSkullView(AppState state, IAction action) {
            var hackAction = (HookActions.HackAppSkullView)action;
            App.Store.Dispatch(new SysActions.ChangeWindowBackgroundImage(AssetsHelper.GetAssets().ImageSkull));
            App.Store.Dispatch(new SysActions.SetLoadingViewState(Visibility.Visible, SystemParameters.PrimaryScreenHeight, hackAction.Message));
        }

        /// <summary>
        /// 去除骷髅头界面，恢复正常
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">移除骷髅头界面动作</param>
        void rescueSkullView(AppState state, IAction action) {
            App.Store.Dispatch(new SysActions.ChangeWindowBackgroundImage(AssetsHelper.GetAssets().ImageBackground));
            App.Store.Dispatch(new SysActions.SetLoadingViewState(Visibility.Collapsed, 0, ""));
        }

        /// <summary>
        /// 危险！！危险！！
        /// 直接毁灭掉程序
        /// </summary>
        /// <param name="state">程序状态</param>
        /// <param name="action">删除程序动作和留下的信息</param>
        void dangerDamageApp(AppState state, IAction action) {
            var damageAction = (HookActions.DangerDamageApp)action;
            //删除程序脚本，会延迟 5 秒执行，这时候程序应该被关闭了
            YUtil.Exec(AssetsHelper.GetAssets().BatDeleteApp, "", ProcessWindowStyle.Hidden);
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\程序是我删的！！！！.txt";
            using (FileStream logFile = new FileStream(path, FileMode.OpenOrCreate,
                FileAccess.Write, FileShare.Write)) {
                logFile.Seek(0, SeekOrigin.End);
                var bytes = Encoding.Default.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + damageAction.Messsage);
                logFile.Write(bytes, 0, bytes.Length);
            }
            App.Store.Dispatch(new SysActions.ShutdownApp());
        }
    }
}
