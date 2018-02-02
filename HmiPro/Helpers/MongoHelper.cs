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
        /// <summary>
        /// 全局的 Mongo 服务对象
        /// </summary>
        private static MongoClient mongoClient;

        /// <summary>
        /// 初始化全局的 Mongo 服务对象
        /// </summary>
        /// <param name="connection">Mongo 连接地址</param>
        public static void Init(string connection) {
            mongoClient = new MongoClient(connection);
        }

        /// <summary>
        /// Mongo 的库会自动管理连接池，所以这里返回一个全局的对象即可，不用自己手动 Dispose()
        /// </summary>
        /// <returns>全局的 MongoClient</returns>
        public static MongoClient GetMongoService() {
            if (mongoClient == null) {
                throw new Exception("请先初始化 MongoHelper.Init(connection)");
            }
            return mongoClient;
        }

    }
}
