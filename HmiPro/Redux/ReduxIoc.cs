using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Sbuscribers;
using HmiPro.Redux.Services;
using Reducto;
using YCsharp.Service;

namespace HmiPro.Redux {

    public class ReduxIoc {
        public static void Init() {
            UnityIocService.AssertIsFirstInject(typeof(ReduxIoc));
            //== 注入 redux 相关数据
            var reducer = new CompositeReducer<AppState>()
                .Part(s => s.CpmState, CpmReducer.Create())
                .Part(s => s.SysState, SysReducer.Create())
                .Part(s => s.MqState, MqReducer.Create());
            ;


            var storePro = new StorePro<AppState>(reducer);

            //== 设置依赖注入
            UnityIocService.RegisterGlobalDepend(storePro);
            UnityIocService.RegisterGlobalDepend<CpmService>();
            UnityIocService.RegisterGlobalDepend<CpmEffects>();
            UnityIocService.RegisterGlobalDepend<SysService>();
            UnityIocService.RegisterGlobalDepend<SysEffects>();
            UnityIocService.RegisterGlobalDepend<MqService>();
            UnityIocService.RegisterGlobalDepend<MqEffects>();
            UnityIocService.RegisterGlobalDepend<MockEffects>();
            UnityIocService.RegisterGlobalDepend<DbEffects>();
            UnityIocService.RegisterGlobalDepend<CpmSubscribers>();
        }

    }
}
