﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using YCsharp.Model.Procotol.SmParam;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var buffer = new byte[] {
                 0x68,0x31,0x30,0x30,0x31,0x38,0x30,0x30,0x31,0x69,0x02,0x66,0x02,0xF4,0x08,0x01,0xF4,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xF5,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xF6,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xF7,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xF8,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xF9,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFA,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFB,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFC,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFD,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFE,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x01,0xFF,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x01,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x02,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x03,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x04,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x05,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x06,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x07,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x08,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x09,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0A,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0B,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0C,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0D,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0E,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x0F,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x10,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x11,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x12,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x13,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x34,0x00,0x01,0x3C,0xB4,0x39,0x58,0x08,0x02,0x35,0x00,0x01,0x3C,0x8B,0x43,0x96,0x08,0x02,0x36,0x00,0x01,0x40,0x39,0x99,0x9A,0x08,0x02,0x37,0x00,0x01,0x3F,0x00,0x00,0x00,0x08,0x02,0x38,0x00,0x01,0x3F,0x00,0x00,0x00,0x08,0x02,0x39,0x00,0x01,0x41,0x20,0x00,0x00,0x08,0x02,0x3A,0x00,0x01,0x41,0x20,0x00,0x00,0x08,0x02,0x3B,0x00,0x01,0x41,0x05,0x3F,0x7D,0x08,0x02,0x3C,0x00,0x01,0x40,0x86,0x56,0x04,0x08,0x02,0x3D,0x00,0x01,0x40,0x07,0x8D,0x50,0x08,0x02,0x3E,0x00,0x01,0x3F,0x88,0xB4,0x39,0x08,0x02,0x3F,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x40,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x41,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x42,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x43,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x44,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x45,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x46,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x47,0x00,0x01,0x00,0x00,0x00,0x00,0x08,0x02,0x14,0x00,0x01,0x43,0x20,0x00,0x00,0x08,0x02,0x15,0x00,0x01,0x43,0x1E,0x00,0x00,0x08,0x02,0x16,0x00,0x01,0x43,0x20,0x00,0x00,0x08,0x02,0x17,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x18,0x00,0x01,0x43,0x1E,0x00,0x00,0x08,0x02,0x19,0x00,0x01,0x43,0x20,0x00,0x00,0x08,0x02,0x1A,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x1B,0x00,0x01,0x43,0x24,0x00,0x00,0x08,0x02,0x1C,0x00,0x01,0x43,0x1E,0x00,0x00,0x08,0x02,0x1D,0x00,0x01,0x43,0x20,0x00,0x00,0x08,0x02,0x1E,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x1F,0x00,0x01,0x43,0x23,0x00,0x00,0x08,0x02,0x20,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x21,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x22,0x00,0x01,0x43,0x21,0x00,0x00,0x08,0x02,0x23,0x00,0x01,0x43,0x23,0x00,0x00,0x08,0x02,0x24,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x25,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x26,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x27,0x00,0x01,0x43,0x28,0x00,0x00,0x08,0x02,0x28,0x00,0x01,0x43,0x28,0x00,0x00,0x08,0x02,0x29,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x2A,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x2B,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x2C,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x2D,0x00,0x01,0x43,0x22,0x00,0x00,0x08,0x02,0x2E,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x2F,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x30,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x31,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x32,0x00,0x01,0x43,0x27,0x00,0x00,0x08,0x02,0x33,0x00,0x01,0x40,0x00,0x00,0x00,0xC0,0x99,0x0D
            };

            var analysis = new SmAnalysis(dynamicBuffer, new LoggerService("C:\\HmiPro\\Log"));
            var models = analysis.ThroughAnalysisStack(buffer, 0, buffer.Length);
            int l = 0;
            foreach (var model in models) {
                model.SmParams.ForEach(sm => {
                    Console.WriteLine(sm.ParamCode + "：" + sm.GetSignalData());
                    try {
                        ++l;
                        sm.GetSignalData();
                    } catch (Exception e) {
                        Console.WriteLine("错误数据：" + sm.GetDataHexStr());
                        Console.WriteLine("错误参数：" + sm.ParamCode);
                    }

                });
            }
            //var str =
            //    "683130303137393031690266034E0801F40A01D0983EDE0801F50A012F683DE10801F60A018E393CE30801F70A01000000000801F80A01000000000801F90A01000000000801FA0A01000000000801FB0A01000000000801FC0A01000000000801FD0A01000000000801FE0A01999A41F10801FF0A01000042540802000A01000000000802010A01000042540802020A01000041400802030A01000000000802040A01000000000802050A01000000000802060A01000000000802070A01000000000802080A01000043C80802090A01555543AD08020A0A01C28F3C7508020B0A010000408008020C0A010000000008020D0A010000000008020E0A010000000008020F0A01000000000802100A01000000000802110A01000000000802120A010FF049740802130A0116F049740802140A0123F049740802150A01B91A461E0802160A0123F049740802170A01000000000802180A01000000000802190A010000000008021A0A010000000008021B0A010000000008021C0A010000000008021D0A01147B410208021E0A010000000008021F0A01000000000802200A01000000000602210A0000000602220A0002490602230A0002000602240101005A060225010100610602260101003A0602270101003F060228010100540602290101006B06022A0101009306022B0101004606022C0101000006022D0101000006022E0101000006022F010100000602300101000006023101010000060232010100000602330101000006023401010000060235010100FA0602360101000006023701010000060238010104E40602390101053A06023A010105B806023B010105E706023C010105DF06023D0101063F06023E0101062106023F010105EA06024001010000060241010100000602420101013706024301010139060244010101380602450101015B0602460101012A0602470101013E060248010101520602490101000006024A0101000006024B0101000006024C0101000006024D0101000006024E0101000006024F01010000060250010100000602510101000006025201010000060253010100000602540101000006025501010000060256010104E206025701010546060258010105AA060259010105DC06025A010105DC06025B0101064006025C0101064006025D0101061806025E0101000006025F010100008E390D";
            //StringBuilder builder = new StringBuilder();
            //string tmp = "";
            //for (int i = 0; i < str.Length; i++) {
            //    if (i % 2 == 0) {
            //        builder.Append(tmp);
            //        tmp = "0x" + str[i];
            //    } else {
            //        tmp += str[i];
            //        if (i != 0 && i != str.Length - 1) {
            //            tmp += ",";
            //        }
            //    }
            //}
            //builder.Append(tmp);
            //Console.WriteLine(builder);
        }
    }
}