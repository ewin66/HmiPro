using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCsharp.Event.Models {
    public class YEventArgs : EventArgs {
        public object Payload { get; set; }

        public YEventArgs() {

        }

        public YEventArgs(object payload) {
            Payload = payload;
        }
    }
}
