

using System;
using System.Collections.Generic;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// MongoDb、InfluxDb 等操作
    /// <date>2017-12-20</date>
    /// <autthor>ychost</autthor>
    /// </summary>
    public static class DbActions {
        //上传采集参数到 InfluxDB
        public static readonly string UPLOAD_CPMS_INFLUXDB = "[Db] Upload Cpms To InfluxDb";
        public static readonly string UPLOAD_CPMS_INFLUXDB_SUCCESS = "[Db] Upload Cpms To InfluxDb Success";
        public static readonly string UPLOAD_CPMS_INFLUXDB_FAILED = "[Db] Upload Cpms To InfluxDb Failed";

        //上传采集参数到 Mongo
        public static readonly string UPLOAD_CPMS_MONGO = "[Db] Upload Cpms To Mongo";
        public static readonly string UPLOAD_CPMS_MONGO_SUCCESS = "[Db] Upload Cpms To Mongo Sucess";
        public static readonly string UPLOAD_CPMS_MONGO_FALIED = "[Db] Upload Cpms To Mongo Failed";

        //上传报警到 Mongo
        public static readonly string UPLOAD_ALARMS_MONGO = "[Db] Upload Alarms To Mongo";
        public static readonly string UPLOAD_ALARMS_MONGO_SUCCESS = "[Db] Upload Alarms To Mongo Success";
        public static readonly string UPLOAD_ALARMS_MONGO_FAILED = "[Db] Upload Alarms To Mongo Failed";

        //上传普通对象到 Mongo
        public static readonly string UPLOAD_DOC_TO_MONGO = "[Db] Upload Document To Mongo";
        public static readonly string UPLOAD_DOC_TO_MONGO_SUCCESS = "[Db] Upload Document To Mongo Success";
        public static readonly string UPLOAD_DOC_TO_MONGO_FAILED = "[Db] Upload Document To Mongo Failed";

        //上传普通集合到 Mongo
        public static readonly string UPLOAD_DOC_MANY_TO_MONGO = "[Db] Upload Document Many To Mongo";
        public static readonly string UPLOAD_DOC_MANY_TO_MONGO_SUCCESS = "[Db] Upload Document Many To Mongo Success";
        public static readonly string UPLOAD_DOC_MANY_TO_MONGO_FAILED = "[Db] Upload Document Many To Mongo Failed";

        public class UploadDocToMongo : IAction {
            public string Type() => UPLOAD_DOC_TO_MONGO;
            public MongoDoc Doc;
            /// <summary>
            /// 不能含有中文
            /// </summary>
            public string Collection;
            /// <summary>
            /// 不能含有中文
            /// </summary>
            public string DbName;

            public UploadDocToMongo(string dbName, string collection, MongoDoc doc) {
                DbName = dbName;
                Collection = collection;
                Doc = doc;
            }
        }


        public class UploadDocManyToMongo : IAction {
            public string Type() => UPLOAD_DOC_MANY_TO_MONGO;
            public string Collection;
            public string DbName;
            public IList<MongoDoc> Docs;

            public UploadDocManyToMongo(string dbName, string collection, IList<MongoDoc> docs) {
                DbName = dbName;
                Collection = collection;
                Docs = docs;
            }
        }

        public struct UploadAlarmsMongo : IAction {
            public string Type() => UPLOAD_ALARMS_MONGO;
            public MqAlarm MqAlarm;
            public string MachineCode;
            public string Collection;

            public UploadAlarmsMongo(string machineCode, string collection, MqAlarm mqAlarm) {
                MachineCode = machineCode;
                Collection = collection;
                MqAlarm = mqAlarm;
            }
        }


        public struct UploadCpmsInfluxDb : IAction {
            public string Type() => UPLOAD_CPMS_INFLUXDB;
            public List<Cpm> Cpms;
            public string MachineCode;

            public UploadCpmsInfluxDb(string machineCode, List<Cpm> cpms) {
                MachineCode = machineCode;
                Cpms = cpms;
            }
        }

        public struct UploadCpmsInfluxDbSuccess : IAction {
            public string Type() => UPLOAD_CPMS_INFLUXDB_SUCCESS;

        }

        public struct UploadCpmsInfluxDbFailed : IAction {
            public string Type() => UPLOAD_CPMS_INFLUXDB_FAILED;
        }

        public struct UploadCpmsMongo : IAction {
            public string Type() => UPLOAD_CPMS_MONGO;
            public List<Cpm> Cpms;
            public string AxisCode;
            public string MachineCode;

            public UploadCpmsMongo(string machineCode, string axisCode, List<Cpm> cpms) {
                MachineCode = machineCode;
                AxisCode = axisCode;
                Cpms = cpms;
            }
        }
    }
}
