using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HmiPro.Helpers {

    /// <summary>
    /// mongo服务创建辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    public static class MongoHelper {
        private static string connection;

        public static void Init(string connection) {
            MongoHelper.connection = connection;
            mongoClient = new MongoClient(connection);
        }

        private static MongoClient mongoClient;

        public static MongoClient GetMongoService() {
            if (mongoClient == null) {
                throw new Exception("请先初始化 MongoHelper.Init(connection)");
            }
            return mongoClient;
        }

    }
}
