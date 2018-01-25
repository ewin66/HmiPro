using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Cores;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using Reducto;
using YCsharp.Service;

namespace HmiPro.Redux {

    /// <summary>
    /// 程序核心逻辑依赖注入
    /// <author>ychost</author>
    /// <date>2017-12-17</date>
    /// </summary>
    public class ReduxIoc {
        public static void Init() {
            UnityIocService.AssertIsFirstInject(typeof(ReduxIoc));
            //== 注入 redux 相关数据
            var reducer = new CompositeReducer<AppState>()
                .Part(s => s.CpmState, CpmReducer.Create())
                .Part(s => s.SysState, SysReducer.Create())
                .Part(s => s.MqState, MqReducer.Create())
                .Part(s => s.AlarmState, AlarmReducer.Create())
                .Part(s => s.OeeState, OeeReducer.Create())
                .Part(s => s.DMesState, DMesReducer.Create())
                .Part(s => s.ViewStoreState, ViewStoreReducer.Create())
                .Part(s => s.DpmStore, DpmReducer.Create());
            ;


            var storePro = new StorePro<AppState>(reducer);

            //== 设置依赖注入
            UnityIocService.RegisterGlobalDepend(storePro);
            UnityIocService.RegisterGlobalDepend<CpmCore>();
            UnityIocService.RegisterGlobalDepend<CpmEffects>();
            UnityIocService.RegisterGlobalDepend<SysService>();
            UnityIocService.RegisterGlobalDepend<SysEffects>();
            UnityIocService.RegisterGlobalDepend<MqService>();
            UnityIocService.RegisterGlobalDepend<MqEffects>();
            UnityIocService.RegisterGlobalDepend<MockEffects>();
            UnityIocService.RegisterGlobalDepend<DbEffects>();
            UnityIocService.RegisterGlobalDepend<DMesCore>();
            UnityIocService.RegisterGlobalDepend<SchCore>();
            UnityIocService.RegisterGlobalDepend<OeeCore>();
            UnityIocService.RegisterGlobalDepend<AlarmCore>();
            UnityIocService.RegisterGlobalDepend<ViewStoreCore>();
            UnityIocService.RegisterGlobalDepend<DpmCore>();
            UnityIocService.RegisterGlobalDepend<PipeEffects>();
            UnityIocService.RegisterGlobalDepend<LoadEffects>();
        }

    }
}
