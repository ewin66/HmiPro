using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Models;

namespace HmiPro.Redux.Actions {
    /// <summary>
    /// 管道操作
    /// <author>ychost</author>
    /// <date>2018-1-22</date>
    /// </summary>
    public static class PipeActions {
        //往管道里面写入字符串
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
