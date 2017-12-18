using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YCsharp.Model.Procotol.SmParam;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议二进制包解析后的模型
    /// 底层->服务器：总包
    /// 总包里面可能包含 SmParam之类的包
    /// fixed:
    ///     2017-10-13 修复多次调用 GetSingleData() 出的Bug
    /// 
    /// </summary>
    public class SmModel {
        public SmModel(SmPackageType type) {
            this.PackageType = type;
        }

        //包类型
        public SmPackageType PackageType { get; set; }
        //模块地址
        //606
        public List<byte> ModuleAddr { get; set; }

        //命令
        public byte Cmd { get; set; }

        //目标类型
        public byte AimType { get; set; }

        /// 参数内容
        public List<SmParam> SmParams { get; set; }

    }



    /// <summary>
    /// 电科智联协议：底层->服务器：采集参数包
    /// <update> 
    ///     2017-10-19  添加 SmParamType 用于区分包的数据类型
    ///
    /// </update>
    /// </summary>
    public class SmParam {
        /// <summary>
        /// 参数地址
        /// </summary>
        public Int16 ParamCode { get; internal set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public byte DataType { get; internal set; }
        /// <summary>
        /// 浮点位置
        /// </summary>
        public byte FloatPlace { get; internal set; }
        /// <summary>
        /// 数据域
        /// </summary>
        public byte[] Data { get; internal set; }

        /// <summary>
        ///保存数值，除去副作用，每次获取数据的时候可能会修改Data
        ///也可以提升一点点性能
        /// </summary>
        private Single? singleData;
        private SmRfid rfidData;
        private SmMultiComStatus multiStatusData;
        private SmSingleStatus? singleStatusData;


        /// <summary>
        /// 主要是依据该类型来获取对应的参数
        /// </summary>
        public SmParamType ParamType {
            get {
                if (IsSignalData()) {
                    return SmParamType.Signal;
                }
                if (IsStrData()) {
                    return SmParamType.String;
                }
                if (IsRfid()) {
                    return SmParamType.Rfid;
                }
                if (IsSingleComStatus()) {
                    return SmParamType.SingleComStatus;
                }
                if (IsMultiComStats()) {
                    return SmParamType.MultiComStatus;
                }
                return SmParamType.Unknown;
            }
        }

        /// <summary>
        /// 是否为浮点类型数据，包括整形
        /// </summary>
        /// <returns></returns>
        public bool IsSignalData() {
            var typeCheck = (DataType == (byte)EmsocketDataType.ReverseFloat ||
                             DataType == (byte)EmsocketDataType.Int ||
                             DataType == (byte)EmsocketDataType.Float ||
                             DataType == (byte)EmsocketDataType.Float3412 ||
                             DataType == (byte)EmsocketDataType.Int3412||
                             DataType == (byte)EmsocketDataType.Float2143
                             );
            return typeCheck;
        }

        /// <summary>
        /// 是否为字符串类型，ASCII
        /// </summary>
        /// <returns></returns>
        public bool IsStrData() {
            return (DataType == (byte)EmsocketDataType.StrAscii);
        }

        /// <summary>
        /// 是否为单个节点的通讯状态
        /// </summary>
        /// <returns></returns>
        public bool IsSingleComStatus() {
            var typeCheck = (DataType == (byte)EmsocketDataType.SingleComStatus);
            return typeCheck;
        }

        /// <summary>
        /// 是否为多节点状态，机台号区分
        /// </summary>
        /// <returns></returns>
        public bool IsMultiComStats() {
            var typeCheck = (DataType == (byte)EmsocketDataType.MultiComstatus);
            return typeCheck;
        }

        /// <summary>
        /// 是否为Rfid格式
        /// </summary>
        /// <returns></returns>
        public bool IsRfid() {
            //有可能是带机台号的标准Rfid数据
            var typeCheck = (DataType == (byte)EmsocketDataType.Rfid);
            return typeCheck;
        }

        /// <summary>
        /// 获取Rfid参数
        /// </summary>
        /// <returns></returns>
        public SmRfid GetRfidData() {

            if (!IsRfid()) {
                throw new Exception("不是RFID读卡数据");
            }
            if (rfidData != null) {
                return rfidData;
            }
            var str = GetStrData();
            var state = (char)str[8];
            rfidData = new SmRfid() {
                MachineAddr = str.Substring(0, 8),
                State = state,
                Rfid = str.Substring(9)
            };
            return rfidData;
        }

        /// <summary>
        /// 字符串都是Ascii编码
        /// </summary>
        /// <returns></returns>
        public string GetStrData() {
            if (!IsStrData()) {
                throw new Exception($"数据类型不是字符串,编码：{ParamCode}");
            }
            return Encoding.ASCII.GetString(Data);
        }

        /// <summary>
        ///获取Data的16进制字符串，方便调试 
        /// </summary>
        /// <returns></returns>
        public string GetDataHexStr() {
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < Data.Length; i++) {
                strB.Append("" + Data[i].ToString("X2"));
            }
            return strB.ToString();
        }

        /// <summary>
        /// 单个节点上不通讯故障状态 (主要是针对一个节点采集)
        /// </summary>
        /// <returns></returns>
        public SmSingleStatus GetSingleComStatus() {
            if (!IsSingleComStatus()) {
                throw new Exception("数据类型不是单点通讯状态");
            }
            if (singleStatusData.HasValue) {
                return singleStatusData.Value;
            }
            //var data = BitConverter.ToInt16(Data.Reverse().ToArray(), 0);
            var data = (int)Data[0];
            SmSingleStatus status = SmSingleStatus.Unknown;
            if (Enum.IsDefined(typeof(SmSingleStatus), data)) {
                status = (SmSingleStatus)Enum.ToObject(typeof(SmSingleStatus), data);
            }
            singleStatusData = status;
            return status;
        }


        /// <summary>
        /// 多个节点通讯状态，机台号区分
        /// </summary>
        /// <returns></returns>
        public SmMultiComStatus GetMultiComStatus() {
            if (!IsMultiComStats()) {
                throw new Exception("数据类型不是多点通讯状态");
            }
            if (multiStatusData != null) {
                return multiStatusData;
            }
            var str = GetStrData();
            var statusStr = str.Substring(8, 1);
            var status = SmMultiStatus.Unknown;
            if (int.TryParse(statusStr, out var intVal)) {
                if (Enum.IsDefined(typeof(SmSingleStatus), intVal)) {
                    status = (SmMultiStatus)Enum.ToObject(typeof(SmMultiStatus), intVal);
                }
            }
            SmMultiComStatus statusObj = new SmMultiComStatus() {
                MachineAddr = str.Substring(0, 8),
                SmSingleStatus = status
            };
            multiStatusData = statusObj;
            return statusObj;
        }

        /// <summary>
        /// 如果数据是浮点类型，则直接获取
        /// </summary>
        /// <returns></returns>
        public Single GetSignalData(int mathRound = 4) {
            if (!this.IsSignalData()) {
                throw new Exception("数据类型不是浮点");
            }
            if (singleData.HasValue) {
                return singleData.Value;
            }

            var dataCopy = new byte[Data.Length];
            Array.Copy(Data, dataCopy, Data.Length);
            double val = 0.00d;
            //逆序浮点计算 1234 ==> 4321
            if (DataType == (byte)EmsocketDataType.ReverseFloat) {
                var data = this.Data.Reverse().ToArray();
                val = BitConverter.ToSingle(data, 0);
                //整形计算
            } else if (DataType == (byte)EmsocketDataType.Int) {
                //转成"12d54f"
                var str = BitConverter.ToString(Data, 0).Replace("-", "");
                //反转
                val = Convert.ToInt32(str, 16);
                //添加小数
                if (FloatPlace > 0) {
                    val = (val / (Math.Pow(10, FloatPlace)));
                }
            } else if (DataType == (byte)EmsocketDataType.Int3412) {
                var tmp = Data[0];
                Data[0] = Data[1];
                Data[1] = tmp;
                tmp = Data[2];
                Data[2] = Data[3];
                Data[3] = tmp;
                val = BitConverter.ToInt32(Data, 0);
                if (FloatPlace > 0) {
                    val = (val / (Math.Pow(10, FloatPlace)));
                }
            } else if (DataType == (byte)EmsocketDataType.Float2143) {
                var tmp = Data[0];
                Data[0] = Data[2];
                Data[2] = tmp;
                tmp = Data[1];
                Data[1] = Data[3];
                Data[3] = tmp;
                val = BitConverter.ToSingle(Data, 0);
            }
              //普通顺序 4321
              else if (DataType == (byte)EmsocketDataType.Float) {
                val = BitConverter.ToSingle(Data, 0);
                //乱序1 3412 ==> 4321
            } else if (DataType == (byte)EmsocketDataType.Float3412) {
                var tmp = Data[0];
                Data[0] = Data[1];
                Data[1] = tmp;
                tmp = Data[2];
                Data[2] = Data[3];
                Data[3] = tmp;
                val = BitConverter.ToSingle(this.Data, 0);
            }
            singleData = (Single)val;
            //复位Data
            Data = dataCopy;
            return (float)Math.Round(val, mathRound);
        }

    }

    /// <summary>
    /// 电科智联协议：服务器->底层 命令包
    /// </summary>
    public class SmSend {
        /// <summary>
        /// 参数编码
        /// </summary>
        public Int16 ParamCode { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public float ParamValue { get; set; }

        public SmSend(Int16 code, float value) {
            this.ParamCode = code;
            this.ParamValue = value;
        }

        /// <summary>
        /// 获取协议包的二进制
        /// </summary>
        /// <returns></returns>
        public byte[] ToPackageBytes() {
            //编码
            var code = BitConverter.GetBytes(this.ParamCode).Reverse();
            //值
            var value = BitConverter.GetBytes(this.ParamValue);
            //类型
            var type = BitConverter.GetBytes((Int16)EmsocketDataType.Float)[0];
            //浮点位置
            var floatPlace = BitConverter.GetBytes(0)[0];
            //数据长度
            var dataLen = BitConverter.GetBytes(value.Length + 4)[0];

            var ans = new List<byte>();
            ans.Add(dataLen);
            ans.AddRange(code);
            ans.Add(type);
            ans.Add(floatPlace);
            ans.AddRange(value);
            return ans.ToArray();
        }
    }

    /// <summary>
    /// 参数类型，可能是浮点也可能是rfid
    /// <author>ychost</author>
    /// <date>2017-10-19</date>
    /// </summary>
    public enum SmParamType {
        Signal,
        Rfid,
        //通讯状态
        SingleComStatus,
        MultiComStatus,
        String,
        StrRfid,
        Unknown
    }



    /// <summary>
    /// RFID卡的格式,带机台号
    /// </summary>
    public class SmRfid {
        /// <summary>
        /// 机台号：8位
        /// </summary>
        public string MachineAddr { get; set; }
        /// <summary>
        /// 状态：1位
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 卡号：12位
        /// </summary>
        public string Rfid { get; set; }

        /// <summary>
        /// 位置，协议判断还是通过参数Code判断？
        /// </summary>
        public SmRfidPos Pos { get; set; } = SmRfidPos.Unknown;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"机台：{MachineAddr} ，状态：{State}，Rfid：{Rfid}";
        }
    }


    /// <summary>
    /// Rfid的位置，放线或者收线
    /// </summary>
    public enum SmRfidPos {
        Unknown,
        Start,
        End
    }

    /// <summary>
    /// 单个通讯状态
    /// </summary>
    public enum SmSingleStatus {

        //通讯正常
        Ok = 0,
        //通讯故障
        Error = 1,
        //通讯状态未知
        Unknown = 2
    }


    /// <summary>
    /// 多节点通讯状态
    /// </summary>
    public enum SmMultiStatus {
        Unknown = -1,
        //有卡
        HaveCard = 1,
        //暂停
        Pause = 2,
        //通讯故障
        ComFaild = 3,
        //多卡
        MultiCard = 4,
        //离线
        Offline = 5,
        //无卡
        NoCard = 6
    }

    /// <summary>
    /// 含机台的通讯状态
    /// </summary>
    public class SmMultiComStatus {
        public string MachineAddr { get; set; }
        public SmMultiStatus SmSingleStatus { get; set; }
    }
}
