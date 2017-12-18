using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Views.Dx;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        public LoggerService Logger;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            //== 各项初始化 ==
            ExceptionHelper.Init();
            configInit(e);
            LoggerHelper.Init(HmiConfig.LogFolder);
            SqliteHelper.Init(HmiConfig.SqlitePath);
            ActiveMqHelper.Init(HmiConfig.MqConn, HmiConfig.MqUserName, HmiConfig.MqUserPwd);
            MongoHelper.Init(HmiConfig.MongoConn);
            InfluxDbHelper.Init($"http://{HmiConfig.InfluxDbIp}:8086", HmiConfig.InfluxCpmDbName);
            AssetsHelper.Init(YUtil.GetAbsolutePath(@".\Profiles\Assets\"));
            ReduxIoc.Init();
            //===============
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }


        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="e"></param>
        void configInit(StartupEventArgs e) {
            Parser.Default.ParseArguments<CmdOptions>(e.Args).WithParsed(opt => {
                opt.ConfigFolder = YUtil.GetAbsolutePath(opt.ConfigFolder);
                YUtil.SetAppAutoStart(GetType().ToString(), bool.Parse(opt.AutoSatrt));
                if (bool.Parse(opt.ShowConsole)) {
                    ConsoleHelper.Show();
                }
                if (bool.Parse(opt.ShowSplash)) {
                    DXSplashScreen.Show<SplashScreenView>();
                    DXSplashScreen.SetState(SplashState.Default);
                }
                HmiConfig.Load(opt.ConfigFolder + @"\Hmi.Config.json");
                HmiConfig.SqlitePath = YUtil.GetAbsolutePath(opt.SqlitePath);


            }).WithNotParsed(err => {
                throw new Exception("参数异常" + e);
            });

            //记录程序崩溃日志
            AppDomain.CurrentDomain.UnhandledException += (s, ue) => {
                Logger.Error("程序崩溃：" + ue.ExceptionObject);
            };
        }
    }

}
