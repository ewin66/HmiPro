# HmiPro

启东数字化改造项目 Hmi 软件

#### 开发工具

* Visual Studio 2017

#### 数据库

* Sqlite V3

  > C:\HmiPro\Store.db

* MongoDb (V3.2.16)

  > 192.168.0.15:27017 (无密码)

* InfluxDb (V1.3)

  > 192.168.0.15:8086 (无密码)

#### 应用部署

直接拷贝程序到 Hmi 电脑上运行即可

* 配置

  1. Global.xls (./Profiles/Global.xls)

     该文件含有逻辑配置、Ip配置、其它配置三大模块

     * 逻辑配置

       配置各个机台的逻辑参数（速度、记米、火花值等等）

     * Ip 配置

       绑定 Hmi 电脑对应的机台，下面加载 Machine.xls 就是根据这里来的

     * 其它配置

       备用

  2. Machine.xls (./Profiles/Prod/Machines/xxx.xls)

     该文件主要配置每个机台的采集参数、计算参数、单位、报警等等

  3. Hmi.Config.Shop.json (./Profiles/Prod/Hmi.Config.Shop.json)

     该文件主要配置程序与服务器对接环境，数据库地址、Mq地址、各项属性等等

     Shop: 车间的环境，Office：启东办公室的环境，Smtc：公司环境的配置

     > 各个环境的网络属性等不一样，所以有多种配置

#### 开发功能说明

##### 项目结构

含三个项目 HmiPro、YUtil、Daemon

1. HmiPro：Hmi软件主要组成部分
2. YUtil：一些静态功能函数库
3. Daemon：程序守护进程（Windows 服务）[备用]

##### 依赖库

程序使用的主要三方库有：

1. DevExpress  （整体框架库，View、ViewModel、Navigation 都是它提供的）
2. Reducto （核心库，一个类似于 Redux 的框架，全局管理了程序的事件数据库）
3. NewtonJson (Json 辅助库，很方便的在 Json 数据与实体对象间进行转化)
4. MongoDb、EntityFramework、Sqlite.CodeFirst（程序的数据库框架）
5. ActiveMq （Mq 通信库）
6. CommandLineParser （启动命令解析库）
7. FluentScheduler （时间调度器）
8. AsyncLock （异步锁可以 lock(async)）
9. Unity （程序的依赖注入库）
10. ... 还有一些库没有列出来，以上是使用频率比较高的库

##### Reducto  (HmiPro.Redux) 

一个类似于 Redux 的框架，掌管了程序的整个数据流和事件，简单示例如下（省去了初始化、依赖注入）：

* 定义 Action

```c#
// HelloActions.cs
public static class HelloAction{
    public static readonly string SAY_HELLO = "[Hello] Say Hello"
      public struct SayHello : IAction{
        public string Type()=>SAY_HELLO;
        public string Message;
        public SayHello(string message = "hello"){
          Message = message;
        }
      }
}
```

* 注册 Core

```c#
// HelloCore.cs
public class HelloCore{
  public IDictionary<string,Action<IAction>> actionExecutors;
  public Logger Logger;
  public void Init(){
    actionExecutors[HelloAction.SAY_HELLO] = (state,action)=>{
      var sayHello = (HelloAction.SayHello)action;
      Logger.Info(sayHello.Message);
    }
    App.Store.Subscribe(actionExecutors);
  }
}
```

* 派发事件

```c#
// Some.cs
App.Store.Dispatch(new HelloActions.SayHello("Hello World"));
```

通过 ```Dispatch``` 就会自动调用 HelloCore.cs 注册的行为 ```Logger.Info(sayHello.Message)```

> 这只是一个简单的使用，还有 Reducer、Effects 层没有接入，具体使用请看源码
>
> 在 Reducto 的基础上打了一些补丁 在 HmiPro.Redux.Patches

##### HmiPro.Redux.Cores 介绍

1. CpmCore : 处理底层参数
2. DMesCore：处理排产、打卡、回传任务数据等
3. AlarmCore：处理产生的报警
4. DpmCore：处理用户填入的自定义参数
5. OeeCore：计算实时 Oee
6. SchCore：程序的定时调度器
7. ViewStoreCore：ViewModel 存储的数据

#### HmiPro 交互

1. 数据交互

   * 底层模块 Tcp 交互（采集数据、报警、线盘卡） 

   * Http 服务器（解析命令） 

   * Mq 交互（接受排产数据、人员打卡、线盘卡、物料卡、机台命令...） 

     ...

2. 行为交互

   * 显示底层模块的数据（数据显示、绘图）

   * 根据配置产生检测报警（报警内容含相关属性）

   * 计算机台的实时 Oee

   * 判断机台落轴并回传落轴数据

     ...

​                      

