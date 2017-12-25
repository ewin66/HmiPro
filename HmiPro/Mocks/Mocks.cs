using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using Newtonsoft.Json;
using YCsharp.Service;

namespace HmiPro.Mocks {
    public static class Mocks {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineCode"></param>
        public static void DispatchMqMockScanMaterial(string machineCode) {
            var data = MqMocks.CreateScanMaterial();
            var mqService = UnityIocService.ResolveDepend<MqService>();
            mqService.ScanMaterialAccept(machineCode, JsonConvert.SerializeObject(data));
            Console.WriteLine("测试扫描物料完毕");
        }
        public static void DispatchMockMqEmpRfid(string machineCode, string type = "上机") {
            var message =
                "{'id':400,'employeeCode':'S71220173321','type':'" + type + "','upTime':'Dec 25, 2017 5:15:16 PM','macCode':'DA','name':'王者归来'}";
            var mqRfid = JsonConvert.DeserializeObject<MqEmpRfid>(message);
            mqRfid.macCode = machineCode;
            var mqService = UnityIocService.ResolveDepend<MqService>();
            mqService.EmpRfidAccept(JsonConvert.SerializeObject(mqRfid));
            Console.WriteLine("发送测试人员打卡数据成功成功");
        }

        public static void DispatchMockMqAxisRfid(string machineCode) {
            var message =
                " {'axis_id':'P71211000061','date':'1514200349659','msg_type':'axis_end','machine_id':'M71207220621','rfids':'P71211000061','newDate':1514200462069,'msgType':'收线','macCode':'ED','name':'王者归来'}";
            var mqRfid = JsonConvert.DeserializeObject<MqAxisRfid>(message);
            mqRfid.macCode = machineCode;
            UnityIocService.ResolveDepend<MqService>().AxisRfidAccpet(JsonConvert.SerializeObject(mqRfid));
            Console.WriteLine("发送测试扫卡Mq数据成功");
        }
    }
}
