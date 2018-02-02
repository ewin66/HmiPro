using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config.Models;
using HmiPro.Redux.Models;
using Newtonsoft.Json;
using SQLite.CodeFirst;

namespace HmiPro.Helpers {
    /// <summary>
    /// Sqlite服务创建辅助类
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    ///<update>
    /// 2018-01-09: 分两种Sqlite,一种每个Hmi都不一定一样可动态给用户设定的（比如配置机台文件路径）
    ///             一种属于全局管理，集中配置的
    /// </update>
    public static class SqliteHelper {
        private static string connection;

        /// <summary>
        /// 设置 Sqlite 文件路径
        /// </summary>
        /// <param name="sqlitePath"></param>
        public static void Init(string sqlitePath) {
            SqliteHelper.connection = $"Data Source={sqlitePath};Pooling=true";
        }
        /// <summary>
        /// using(var ctx = SqliteHelper.CreateSqliteService()){}
        /// </summary>
        /// <returns></returns>
        public static SqliteService CreateSqliteService() {
            if (string.IsNullOrEmpty(connection)) {
                throw new Exception("请先初始化 SqliteHelper.Init(sqlitePath)");
            }
            return new SqliteService(connection);
        }

        /// <summary>
        /// 异步操作Sqlite
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task DoAsync(Action<SqliteService> action) {
            return Task.Run(() => {
                using (var ctx = CreateSqliteService()) {
                    action(ctx);
                }
            });
        }
    }

    /// <summary>
    /// Sqlite操作封装，比如持久化配置数据等等
    /// 添加Sqlite.CodeFirst让Sqlite也支持CodeFirst
    /// <date>2017-09-30</date>
    /// <author>ychost</author>
    /// </summary>
    public class SqliteService : DbContext {
        /// <summary>
        /// 写数据的时候上锁，有时候同时写一张表，同一个key，会出错
        /// 主要是针对 Persists 这张表
        /// </summary>
        static readonly object persistLock = new object();

        /// <inheritdoc />
        /// <summary>
        /// 直接指定连接字符串，便于约定数据库位置
        /// "Data Source=C:\\Users\\ychost\\Desktop\\workdspace\\smtc\\wpf-Smtc-PanYu-Monitor\\PanYuMonitor\\PanYuConfig\\v1\\sqlite\\panyu.db;Pooling=true
        /// <provider invariantName="System.Data.SQLite" type="System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6" />
        ///  </summary>
        /// <param name="conStr"></param>
        public SqliteService(string conStr) : base(createConnection(conStr), true) {
            this.Configure();
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="connStr"></param>
        /// <returns></returns>
        static DbConnection createConnection(string connStr) {
            var conn = DbProviderFactories.GetFactory("System.Data.SQLite").CreateConnection();
            conn.ConnectionString = connStr;
            return conn;
        }

        /// <summary>
        /// 配置懒加载
        /// </summary>
        private void Configure() {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        /// <summary>
        /// 初始化模式为 SqliteDropCreateDatabaseWhenModelChanges 
        /// 目前 Sqlite 的 CodeFirst 还不支持 Merge
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            Database.SetInitializer<SqliteService>(new SqliteContextInit(modelBuilder));
        }

        /// <summary>
        /// 该表为一个 Json 数据表，通用模式
        /// </summary>
        public DbSet<Persist> Persists { get; set; }
        /// <summary>
        /// Hmi的设置表，不过目前已经没用了，全都转移到 Global.xls 同一配置
        /// </summary>
        public DbSet<Setting> Settings { get; set; }
        /// <summary>
        /// 上传失败的落轴数据
        /// </summary>
        public DbSet<MqUploadManu> UploadManuFailures { get; set; }

        /// <summary>
        /// 保存一个 Json 对象到表中
        /// </summary>
        /// <param name="persist"></param>
        public void SavePersist(Persist persist) {
            lock (persistLock) {
                Persists.AddOrUpdate(persist);
                this.SaveChanges();
            }
        }

        /// <summary>
        /// 根据 Key 移除一个 Json 对象
        /// </summary>
        /// <param name="key"></param>
        public void RemovePersist(string key) {
            lock (persistLock) {
                var delPersist = Persists.FirstOrDefault(s => s.Key == key);
                if (delPersist != null) {
                    Persists.Remove(delPersist);
                }
                SaveChanges();
            }
        }

        /// <summary>
        /// 保存一批 Json 对象到表中
        /// </summary>
        /// <param name="persists"></param>
        public void SavePersist(IList<Persist> persists) {
            lock (persistLock) {
                Persists.AddRange(persists);
                this.SaveChanges();
            }
        }

        /// <summary>
        /// 将表中的 Json 对象反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Restore<T>(string key) {
            lock (persistLock) {
                var ret = default(T);
                var persist = Persists.FirstOrDefault(p => p.Key == key);
                if (persist != null) {
                    var json = persist.Json;
                    ret = JsonConvert.DeserializeObject<T>(json);
                }
                return ret;
            }
        }

        /// <summary>
        /// 直接返回表中的 Json 字符串，不进行反序列化
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Restore(string key) {
            lock (persistLock) {
                var persist = Persists.Where(p => p.Key == key).Take(1).FirstOrDefault();
                return persist?.Json;
            }
        }
    }


    /// <summary>
    /// sqlite初始化策略
    /// <date>2017-07-18</date>
    /// <author>ychost</author>
    /// </summary>
    class SqliteContextInit : SqliteDropCreateDatabaseWhenModelChanges<SqliteService> {
        public SqliteContextInit(DbModelBuilder modelBuilder) : base(modelBuilder) {

        }
    }


    /// <summary>
    /// 序列化实体
    /// </summary>
    public class Persist {
        /// <summary>
        /// 每个 Json 字符串都有一个独有的 Key
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Key { get; set; }

        /// <summary>
        /// 序列化后的字符串
        /// </summary>
        public string Json { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 初始化更新时间
        /// </summary>
        public Persist() {
            UpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 常有构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="json"></param>
        public Persist(string key, string json) {
            Key = key;
            Json = json;
            UpdateTime = DateTime.Now;
        }
    }
}
