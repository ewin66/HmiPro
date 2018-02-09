using Microsoft.VisualStudio.TestTools.UnitTesting;
using YCsharp.Model.Procotol.SmParam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Model.Buffers;
using YCsharp.Service;

namespace YCsharp.Model.Procotol.SmParam.Tests {
    [TestClass()]
    public class SmAnalysisTests {
        [TestMethod()]
        public void ThroughAnalysisStackTest() {
            //Assert.Fail();
            var dynamicBuffer = new YDynamicBuffer(1024);
            var buffer = new byte[] {0x68,0x39,0x39,0x39,0x39,0x39,0x39,0x39,0x39,0x69,0x02,0x66,0x00,0x09,0x08,0x01,0xf4,0x0a,0x00,0x00,0x00,0x44,0x2d,0xaf,0xf1,0x0d
            };
            var analysis = new SmAnalysis(dynamicBuffer, new LoggerService("C:\\HmiPro\\Log"));
            var models = analysis.ThroughAnalysisStack(buffer, 0, buffer.Length);
            foreach (var model in models) {
                model.SmParams.ForEach(sm => {
                    Console.WriteLine(sm.GetSignalData());
                });
            }

        }
    }
}