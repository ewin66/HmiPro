using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Service;

namespace HmiPro.Helpers {
    public static class ActiveMqHelper {
        private static string connection;
        private static string user;
        private static string password;

        static ActiveMqService activeMqService;
        public static void Init(string connection, string user, string password) {
            ActiveMqHelper.connection = connection;
            ActiveMqHelper.user = user;
            ActiveMqHelper.password = password;
            activeMqService = new ActiveMqService(ActiveMqHelper.connection, ActiveMqHelper.user, ActiveMqHelper.password);
        }

        public static ActiveMqService GetActiveMqService() {
            if (activeMqService == null) {
                throw new Exception("请先初始化 ActiveMqHelper.Init(connection,user,,password)");
            }
            return activeMqService;
        }


    }
}
