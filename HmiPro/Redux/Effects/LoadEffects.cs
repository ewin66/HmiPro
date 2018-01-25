using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Models;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Effects {
    public class LoadEffects {
        public StorePro<AppState>.AsyncActionNeedsParam<LoadActions.LoadMachieConfig, bool> LoadMachineConfig;
        public LoggerService Logger;

        public LoadEffects() {
            initLoadMachineConfig();
            Logger = LoggerHelper.CreateLogger(GetType().ToString());
        }

        void initLoadMachineConfig() {
            LoadMachineConfig = App.Store.asyncAction<LoadActions.LoadMachieConfig, bool>(
                async (dispatch, getState, instance) => {
                    dispatch(instance);
                    return await Task.Run(() => {
                        if (HmiConfig.IsDevUserEnv) {
                            loadConfigBySetting();
                        } else {
                            loadConfigByDefine();
                        }
                        return true;
                    });
                });
        }

        /// <summary>
        /// 通过 Sqlite 中的设置数据来加载配置
        /// </summary>
        private void loadConfigBySetting() {
            using (var ctx = SqliteHelper.CreateSqliteService()) {
                var setting = ctx.Settings.ToList().LastOrDefault();
                if (setting == null) {
                    App.Store.Dispatch(new SysActions.ShowSettingView());
                } else {
                    try {
                        MachineConfig.Load(setting.MachineXlsPath);
                        checkConfig();
                        afterConfigLoaded();
                    } catch (Exception e) {
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "配置出错",
                            Content = e.Message
                        }));
                        Logger.Error($"程序配置有误", e);
                        App.Store.Dispatch(new SysActions.ShowSettingView());
                    }
                }
            }
        }

        /// <summary>
        /// 通过 Global.xls中预定义的数据来加载配置
        /// </summary>
        void loadConfigByDefine() {
            try {
                MachineConfig.LoadFromGlobal();
                checkConfig();
                afterConfigLoaded();
            } catch (Exception e) {
                Logger.Error("程序配置出错", e);
                App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                    Title = "配置出错",
                    Content = e.Message
                }));
                MessageBox.Show("读取配置失败，请检查网络连接", "网络异常", MessageBoxButton.OK, MessageBoxImage.None);
                App.Store.Dispatch(new SysActions.ShutdownApp());
            }
        }

        /// <summary>
        /// 配置文件加载成功之后执行的一些初始化
        /// </summary>
        async void afterConfigLoaded() {
            //== 初始化部分State
            App.Store.Dispatch(new ViewStoreActions.Init());
            App.Store.Dispatch(new CpmActions.Init());
            App.Store.Dispatch(new AlarmActions.Init());
            App.Store.Dispatch(new OeeActions.Init());
            App.Store.Dispatch(new DMesActions.Init());
            App.Store.Dispatch(new DpmActions.Init());

            var sysEffects = UnityIocService.ResolveDepend<SysEffects>();
            var cpmEffects = UnityIocService.ResolveDepend<CpmEffects>();
            var mqEffects = UnityIocService.ResolveDepend<MqEffects>();
            //连接服务器
            mqEffects.Start();

            UnityIocService.ResolveDepend<DMesCore>().Init();
            UnityIocService.ResolveDepend<AlarmCore>().Init();
            UnityIocService.ResolveDepend<CpmCore>().Init();
            UnityIocService.ResolveDepend<OeeCore>().Init();
            UnityIocService.ResolveDepend<DpmCore>().Init();
            await UnityIocService.ResolveDepend<SchCore>().Init();

            //启动 Http 服务系统
            var starHttpSystem = App.Store.Dispatch(sysEffects.StartHttpSystem(new SysActions.StartHttpSystem($"http://+:{HmiConfig.CmdHttpPort}/")));
            //启动 参数采集 系统
            var startCpmServer = App.Store.Dispatch(cpmEffects.StartServer(new CpmActions.StartServer(HmiConfig.CpmTcpIp, HmiConfig.CpmTcpPort)));

            //== 启动一些 Mq 的监听任务
            Dictionary<string, Task<bool>> startListenMqDict = new Dictionary<string, Task<bool>>();
            foreach (var pair in MachineConfig.MachineDict) {
                //监听排产任务
                var stQueueName = @"QUEUE_" + pair.Key;
                var stTask = App.Store.Dispatch(mqEffects.StartListenSchTask(new MqActions.StartListenSchTask(pair.Key, stQueueName)));
                startListenMqDict[stQueueName] = stTask;
                //监听来料
                var smQueueName = $@"JUDGE_MATER_{pair.Key}";
                var smTask = App.Store.Dispatch(mqEffects.StartListenScanMaterial(new MqActions.StartListenScanMaterial(pair.Key, smQueueName)));
                startListenMqDict[smQueueName] = smTask;

            }
            //监听人员打卡
            var empRfidTask = App.Store.Dispatch(mqEffects.StartListenEmpRfid(new MqActions.StartListenEmpRfid(HmiConfig.TopicEmpRfid)));
            startListenMqDict["rfidEmpTask"] = empRfidTask;

            //监听轴号卡
            var axisRfidTask =
                App.Store.Dispatch(
                    mqEffects.StartListenAxisRfid(new MqActions.StartListenAxisRfid(HmiConfig.TopicListenHandSet)));
            startListenMqDict["rfidAxisTask"] = axisRfidTask;

            var tasks = new List<Task<bool>>() { starHttpSystem, startCpmServer };
            tasks.AddRange(startListenMqDict.Values);

            //检查各项任务启动情况
            await Task.Run(() => {
                //等等所有任务完成
                var timeout = 60000;
                if (CmdOptions.GlobalOptions.MockVal) {
                    timeout = 3000;
                }
                var isStartedOk = Task.WaitAll(tasks.ToArray(), timeout);
                if (!isStartedOk) {
                    App.Store.Dispatch(new SysActions.AddMarqueeMessage(SysActions.MARQUEE_APP_START_TIMEOUT,
                        "软件启动超时，请检查网络连接"));
                    if (CmdOptions.GlobalOptions.MockVal) {
                        App.Store.Dispatch(new SysActions.AppInitCompleted());
                    }
                    return;
                }

                //是否启动完成Cpm服务
                var isCpmServer = startCpmServer.Result;
                if (!isCpmServer) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动失败",
                        Content = $"参数采集服务 {HmiConfig.CpmTcpIp}:{HmiConfig.CpmTcpPort} 启动失败，请检查"
                    }));
                }
                //是否启动完成Http解析系统
                var isHttpSystem = starHttpSystem.Result;
                if (!isHttpSystem) {
                    App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "启动失败",
                        Content = $"Http {HmiConfig.CmdHttpPort} 端口服务启动失败，请检查"
                    }));
                }
                //是否完成监听Mq
                foreach (var pair in startListenMqDict) {
                    var isStartListenMq = pair.Value.Result;
                    if (pair.Key.ToUpper().Contains("QUEUE") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                            new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 排产队列 {pair.Key} 失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("JUDGE_MATER") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                            new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 扫描来料队列 {pair.Key} 失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("RFIDEMP") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                             new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 人员打卡 数据失败，请检查" }));

                    } else if (pair.Key.ToUpper().Contains("RFIDAXIS") && (!isStartListenMq)) {
                        App.Store.Dispatch(new SysActions.ShowNotification(
                             new SysNotificationMsg() { Title = "启动失败", Content = $"监听Mq 线盘卡失败，请检查" }));
                    }
                }

                var version = YUtil.GetAppVersion(Assembly.GetExecutingAssembly());
                App.Store.Dispatch(
                    new SysActions.ShowNotification(new SysNotificationMsg() {
                        Title = "系统启动完毕",
                        Content = $"版本:{version}"
                    }));
                //初始化完成
                App.Store.Dispatch(new SysActions.AppInitCompleted());
            });
        }

        /// <summary>
        /// 检查配置
        /// </summary>
        void checkConfig() {

        }
    }
}
