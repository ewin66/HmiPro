﻿

using System;
using System.Collections.Generic;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 相聚看的相关操作
    /// <date>2017-12-20</date>
    /// <autthor>ychost</autthor>
    /// </summary>
    public static class DbActions {
        public static readonly string UPLOAD_CPMS_INFLUXDB = "[Db] Upload Cpms To InfluxDb";
        public static readonly string UPLOAD_CPMS_INFLUXDB_SUCCESS = "[Db] Upload Cpms To InfluxDb Success";
        public static readonly string UPLOAD_CPMS_INFLUXDB_FAILED = "[Db] Upload Cpms To InfluxDb Failed";
        public static readonly string UPLOAD_CPMS_MONGO = "[Db] Upload Cpms To Mongo";
        public static readonly string UPLOAD_ALARMS_MONGO = "[Db] Upload Alarms To Mongo";

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
