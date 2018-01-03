using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YCsharp.Model.Procotol.SmParam {
    /// <summary>
    /// 电科智联协议帧
    /// </summary>
    internal enum SmFrame {
        //帧头
        Start = 0x68,
        //固定帧
        Fixed = 0x69,
        //结束帧
        End = 0x0d,
        //类型帧：工业
        IndustryAimType = 0x02
    }

    internal enum SmIndex {
        //帧头
        Start = 0,
        //机台地址
        MachineAddrStart = 1,
        MacineAddrCount = 8,
        //固定帧
        Fixed = 9,
        //类型帧
        //工业类型，科技类型等等
        AimType = 10,
        //命令
        Cmd = 11,
        //数据总长度
        TotalLenStart = 12,
        TotalLenCount = 2,
        //属性总长度，数据域+4byte
        PropLength = 14,
        //数据起始地址
        DataStart = 19,
        //传感器地址
        SensorAddrStart = 15,
        SensorAddrCount = 2,
        //数据类型
        DataType = 17,
        //浮点位置
        FloatPlace = 18,
    }

    /// <summary>
    /// 包类型
    /// </summary>
    public enum SmPackageType {
        //普通包
        ParamPackage = 0,
        //心跳包
        HeartbeatPackage = 1,
        //应答包
        ReplyPackage = 2,
        //主机发送包客户端回复的包
        ClientReplyCmd = 3,
        //解析不了的包
        ErrorPackage = 4
    }

    /// <summary>
    /// 补充协议
    /// </summary>
    public enum SmSup {
        //所有包最小长度
        AllPackageMinLength = 15,
        //最短长度
        MinLength = 21,
        //除去数据域的长度,协议制定
        FixedLength = 2
    }

    /// <summary>
    /// 用命令区分是什么包
    /// </summary>
    public enum SmCmdFrame {
        Param = 0x66,
        Heartbeat = 0x6a,
        ClientReplyCmd = 0xeb
    }

    public enum EmsocketDataType {
        //逆序浮点 1234 ==> 4321
        //注释：在byte中的逆序为转换的时候需要reverse
        ReverseFloat = 0,
        //整形
        Int = 1,
        //普通浮点 4321 ==>4321
        Float4321 = 3,
        //整形逆序：3421==>4321
        Int3412 = 4,
        //乱序浮点1
        //3412 ===>4321
        Float3412 = 5,
        //2143==>4321
        Float2143 = 6,
        //字符串类型，应用Ascii转换
        StrAscii = 2,
        //8位机台号+1状态号+12位卡号
        Rfid = 16,
        //单个通讯状态
        SingleComStatus = 15,
        //多个节点通讯状态，含机台号区分
        MultiComstatus = 254
    }

    /// <summary>
    /// 应答包规定帧
    /// </summary>
    enum SmReplyFrame {
        Start = 0x68,
        Fixed = 0x69,
        //类型帧：工业
        IndustryAimType = 0x02,
        End = 0x0d
    }

    /// <summary>
    /// 应答包帧索引
    /// </summary>
    enum SmReplyIndex {
        Start = 0,
        MachineAddrStart = 1,
        MachineAddrCount = 8,
        Fixed = 9,
        AimType = 10,
        Cmd = 11,
        DataLengthStart = 12,
        DataLengthCount = 2,
        //CrcStart = 14,
        //CrcCount = 2,
        //End = 16
    }

    /// <summary>
    /// 应答包补充定义
    /// </summary>
    enum SmReplySup {
        //除去数据域的包长
        ExceptDataLength = 17
    }

    /// <summary>
    /// 给客户端主动发送的命令
    /// </summary>
    public enum SmActionFrame {
        Alarm = 0x6b
    }


    /// <summary>
    /// 主动发送动作
    /// </summary>
    public enum SmAction {
        //打开报警
        AlarmOpen = 1,
        //关闭报警
        AlarmClose = 0,
        //不执行任何动作，用作置位
        NoAction = -1,
    }

    /// <summary>
    /// 加密表包索引
    /// </summary>
    enum SmEncryptIndex {
        Start = 0,
        ModuleAddrStart = 1,
        ModuleAddrCount = 8,
        Fixed = 9,
        AimType = 10,
        Cmd = 11,
        DataLenStart = 12,
        DataLenCount = 2,
        //已完成字节
        CompleteByte = 14,
        //数据起始位
        DataStart = 15,
    }

    /// <summary>
    /// 加密表包帧
    /// </summary>
    enum SmEncryptFrame {
        Start = 0x68,
        Fixed = 0x69,
        AimType = 0x02,
        Cmd = 0x6a,
        End = 0x0d
    }

    enum SmEncryptSup {
        //除去数据域的包长度
        ExceptDataPackageLen = 18
    }
}
