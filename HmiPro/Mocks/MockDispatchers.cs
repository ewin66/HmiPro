using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;
using HmiPro.Redux.Models;
using HmiPro.Redux.Services;
using Newtonsoft.Json;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Mocks {
    public static class MockDispatchers {
        /// <summary>
        /// 扫描物料模拟
        /// </summary>
        /// <param name="machineCode"></param>
        public static void DispatchMqMockScanMaterial(string machineCode) {
            var data = MqMocks.CreateScanMaterial();
            var mqService = UnityIocService.ResolveDepend<MqService>();
            mqService.ScanMaterialAccept(machineCode, JsonConvert.SerializeObject(data));
            Console.WriteLine("测试扫描物料完毕");
        }
        /// <summary>
        /// 人员打卡模拟
        /// </summary>
        /// <param name="machineCode"></param>
        /// <param name="type"></param>
        public static void DispatchMockMqEmpRfid(string machineCode, string type = "上机") {
            var message =
                "{'id':400,'employeeCode':'S71220173321','type':'" + type + "','upTime':'Dec 25, 2017 5:15:16 PM','macCode':'DA','name':'李明'}";
            var mqRfid = JsonConvert.DeserializeObject<MqEmpRfid>(message);
            mqRfid.macCode = machineCode;
            var mqService = UnityIocService.ResolveDepend<MqService>();
            mqService.EmpRfidAccept(JsonConvert.SerializeObject(mqRfid));
            Console.WriteLine("发送测试人员打卡数据成功成功");
        }

        /// <summary>
        /// 扫描线盘卡模拟
        /// </summary>
        /// <param name="machineCode"></param>
        public static void DispatchMockMqAxisRfid(string machineCode) {
            var message =
                " {'axis_id':'P71211000061','date':'1514200349659','msg_type':'axis_end','machine_id':'M71207220621','rfids':'P71211000061','newDate':1514200462069,'msgType':'收线','macCode':'ED','name':'王者归来'}";
            var mqRfid = JsonConvert.DeserializeObject<MqAxisRfid>(message);
            mqRfid.macCode = machineCode;
            UnityIocService.ResolveDepend<MqService>().AxisRfidAccpet(JsonConvert.SerializeObject(mqRfid));
            Console.WriteLine("发送测试扫卡Mq数据成功");
        }

        /// <summary>
        /// 报警模拟
        /// </summary>
        /// <param name="code"></param>
        public static void DispatchMockAlarm(int code) {
            foreach (var pair in MachineConfig.MachineDict) {
                var machineCode = pair.Key;
                App.Store.Dispatch(new AlarmActions.GenerateOneAlarm(machineCode, AlarmMocks.CreateOneAlarm(code)));
            }
        }


        /// <summary>
        /// 派发模拟排产任务
        /// </summary>
        public static void DispatchMockSchTask(string machineCode, int id = 0) {
            var mockEffects = UnityIocService.ResolveDepend<MockEffects>();
            var task = YUtil.GetJsonObjectFromFile<MqSchTask>(AssetsHelper.GetAssets().MockMqSchTaskJson);
            task.workcode = YUtil.GetRandomString(8);
            task.id = id;
            task.maccode = machineCode;
            foreach (var axis in task.axisParam) {
                axis.maccode = task.maccode;
                axis.axiscode = YUtil.GetRandomString(10);
            }
            JavaTime startTime = new JavaTime() {
                time = YUtil.GetUtcTimestampMs(YUtil.GetRandomTime(DateTime.Now.AddDays(-1), DateTime.Now))
            };
            task.pstime = startTime;
            task.pdtime = startTime;
            App.Store.Dispatch(mockEffects.MockSchTaskAccept(new MockActions.MockSchTaskAccpet(task)));
        }
    }
}
