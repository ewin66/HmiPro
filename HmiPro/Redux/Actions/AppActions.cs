using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Reducers;

namespace HmiPro.Redux.Actions {

    /// <summary>
    /// 所有 Action 都必须带 Type() 好甄别
    /// <author>ychost</author>
    /// <date>2017-12-19</date>
    /// </summary>
    public interface IAction {
        string Type();
    }

    /// <summary>
    /// 一个简单的 Action，几乎不怎么用
    /// </summary>
    public struct SimpleAction : IAction {
        private readonly string type;
        public string Type() {
            return type;
        }
        public SimpleAction(string type, object payload = null) {
            this.type = type;
            Payload = payload;
        }

        public object Payload;
    }

}
