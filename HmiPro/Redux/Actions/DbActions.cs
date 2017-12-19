

using System.Collections.Generic;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    public static class DbActions {
        public static readonly string UPLOAD_CPMS_INFLUXDB = "[Db] Upload Cpms To InfluxDb";

        public struct UploadCpmsInfluxDb : IAction {
            public string Type() => UPLOAD_CPMS_INFLUXDB;
            public List<Cpm> Cpms;
            public string MachineCode;

            public UploadCpmsInfluxDb(string machineCode, List<Cpm> cpms) {
                MachineCode = machineCode;
                Cpms = cpms;
            }
        }
    }
}
