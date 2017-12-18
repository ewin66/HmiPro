using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Reducers;

namespace HmiPro.Redux.Actions {

    public interface IAction {
        string Type();
    }

    public class SimpleAction : IAction {
        private readonly string type;
        public string Type() {
            return type;
        }
        public SimpleAction(string type) {
            this.type = type;
        }
    }

}
