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

        public static void Init(string sqlitePath) {
            SqliteHelper.connection = $"Data Source={sqlitePath};Pooling=true";
        }

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


        private void Configure() {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            Database.SetInitializer<SqliteService>(new SqliteContextInit(modelBuilder));
        }


        public DbSet<Persist> Persists { get; set; }
        public DbSet<Setting> Settings { get; set; }
        /// <summary>
        /// 上传失败的落轴数据
        /// </summary>
        public DbSet<MqUploadManu> UploadManuFailures { get; set; }

        public void SavePersist(Persist persist) {
            Persists.AddOrUpdate(persist);
            this.SaveChanges();
        }

        public void SavePersist(IList<Persist> persists) {
            Persists.AddRange(persists);
            this.SaveChanges();
        }

        public T Restore<T>(string key) {
            var ret = default(T);
            var persist = Persists.FirstOrDefault(p => p.Key == key);
            if (persist != null) {
                var json = persist.Json;
                ret = JsonConvert.DeserializeObject<T>(json);
            }
            return ret;
        }

        public string Restore(string key) {
            var persist = Persists.Where(p => p.Key == key).Take(1).FirstOrDefault();
            return persist?.Json;
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

        public Persist() {
            UpdateTime = DateTime.Now;
        }

        public Persist(string key, string json) {
            Key = key;
            Json = json;
            UpdateTime = DateTime.Now;
        }
    }
}
