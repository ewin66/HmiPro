using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using YCsharp.Service;

namespace HmiPro.Helpers {
    public static class ActiveMqHelper {
        /// <summary>
        /// Mq 链接 URL
        /// </summary>
        private static string connection;
        /// <summary>
        /// Mq 登录名
        /// </summary>
        private static string user;
        /// <summary>
        /// Mq 登录密码
        /// </summary>
        private static string password;
        /// <summary>
        /// 封装的 Mq 的相关操作
        /// </summary>
        static ActiveMqService activeMqService;
        /// <summary>
        /// 初始化，生成一个全局唯一的 activeMqService
        /// </summary>
        /// <param name="connection">链接地址</param>
        /// <param name="user">登录名</param>
        /// <param name="password">登录密码</param>
        public static void Init(string connection, string user, string password) {
            ActiveMqHelper.connection = connection;
            ActiveMqHelper.user = user;
            ActiveMqHelper.password = password;
            activeMqService = new ActiveMqService(ActiveMqHelper.connection, ActiveMqHelper.user, ActiveMqHelper.password, TimeSpan.FromSeconds(HmiConfig.MqSendRequestTimeoutSec));
        }

        /// <summary>
        /// 获取全局的 ActiveMqService 
        /// 会自动管理连接池
        /// </summary>
        /// <returns></returns>
        public static ActiveMqService GetActiveMqService() {
            if (activeMqService == null) {
                throw new Exception("请先初始化 ActiveMqHelper.Init(connection,user,password)");
            }
            return activeMqService;
        }


    }
}
