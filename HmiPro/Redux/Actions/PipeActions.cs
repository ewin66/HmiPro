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
        public static readonly string WRITE_CMD = "[Pipe] Write Cmd";
        public static readonly string WRITE_CMD_SUCCESS = "[Pipe] Write Cmd Success";
        public static readonly string WRITE_CMD_FAILED = "[Pipe] Write Cmd Failed";

        public struct WriteCmd : IAction {
            public string Type() => WRITE_CMD;
            public PipeCmd Cmd;
            public string PipeName;
            public string PipeServerName;

            public WriteCmd(PipeCmd cmd, string pipeName, string pipeServerName = ".") {
                Cmd = cmd;
                PipeName = pipeName;
                PipeServerName = pipeServerName;
            }
        }
    }
}
