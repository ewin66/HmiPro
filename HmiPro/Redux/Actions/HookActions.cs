using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Redux.Actions {
    /// <summary>
    ///  「后门」 动作，开放出一些有意思的「钩子」
    /// <author>Hacker</author>
    /// <date>2018-2-1</date>
    /// </summary>
    public static class HookActions {

        //将程序变成骷髅头界面，但是程序后台逻辑并不停止
        public static readonly string HACK_APP_SKULL_VIEW = "[Hook] Hack App Skull View";

        //恢复正常程序界面，移除骷颅头界面
        public static readonly string RESCUE_APP_SKULL_VIEW = "[Hook] Rescue App Skull View";

        //危险！！前方高能！！！前方高能！！！!不可恢复！！！！
        //直接毁掉程序，不到万不得已不要使用！！
        //最后的底线，应该买个大按钮来执行此操作
        public static readonly string DANGER_DAMAGE_APP = "[Hook] Danger Damage App";

        /// <summary>
        /// 使用之前请阅读《莫生气》
        /// </summary>
        public struct DangerDamageApp : IAction {
            public string Type() => DANGER_DAMAGE_APP;
            /// <summary>
            /// 毁灭程序总得留下点什么
            /// </summary>
            public string Messsage;

            /// <summary>
            /// 使用之前请阅读《莫生气》
            /// </summary>
            /// <param name="message"></param>
            public DangerDamageApp(string message) {
                Messsage = message;
            }
        }


        public struct RescueAppSkullView : IAction {
            public string Type() => RESCUE_APP_SKULL_VIEW;
        }

        public struct HackAppSkullView : IAction {
            public string Type() => HACK_APP_SKULL_VIEW;
            /// <summary>
            /// 骷髅头里面的文字提示信息
            /// </summary>
            public string Message;

            public HackAppSkullView(string message) {
                Message = message;
            }
        }

    }
}
