using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    public static class PipeActions {
        public static readonly string WRITE_STRING = "[Pipe] Write String";
        public static readonly string WRITE_STRING_SUCCESS = "[Pipe] Write String Success";
        public static readonly string WRITE_STRING_FAILED = "[Pipe] Write String Failed";

        public struct WriteRest : IAction {
            public string Type() => WRITE_STRING;
            public PipeRest RestData;
            public string PipeName;
            public string PipeServerName;

            public WriteRest(PipeRest restData, string pipeName, string pipeServerName = ".") {
                RestData = restData;
                PipeName = pipeName;
                PipeServerName = pipeServerName;
            }
        }
    }
}
