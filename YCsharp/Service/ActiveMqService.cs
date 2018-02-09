using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Apache.NMS.Util;
using YCsharp.Util;

namespace YCsharp.Service {
    /// <summary>
    /// ActiveMq中间件封装
    /// <date>2017-09-30</date>
    /// <author>ychost</author>
    /// </summary>
    /// <update>
    /// <date>2018-01-20</date>
    /// 
    /// </update>
    public class ActiveMqService {
        private readonly string mqConn;
        private readonly string mqUserName;
        private readonly string mqUserPwd;
        private readonly TimeSpan requestTimeout;
        private IConnectionFactory poolFactory;
        private IConnection poolConnection;
        /// <summary>
        /// 是否启动了
        /// </summary>
        public bool IsStarted => poolConnection?.IsStarted ?? false;

        public ActiveMqService(string mqConn, string mqUserName, string mqUserPwd, TimeSpan requestTimeout) {
            this.mqConn = mqConn;
            this.mqUserName = mqUserName;
            this.mqUserPwd = mqUserPwd;
            this.requestTimeout = requestTimeout;
        }

        /// <summary>
        /// 请务必先调用该方法
        /// </summary>
        public void Start() {
            Uri uri = new Uri(mqConn);
            poolFactory = new ConnectionFactory(uri);
            poolConnection = poolFactory.CreateConnection(mqUserName, mqUserPwd);
            poolConnection.ClientId = Guid.NewGuid().ToString();
            poolConnection.RequestTimeout = requestTimeout;
            poolConnection.Start();
        }

        /// <summary>
        /// 发送主题
        /// </summary>
        public async Task PublishOneTopicAsync(string topic, string message) {
            await Task.Run(() => {
                this.PulishOneTopic(topic, message);
            });
        }

        /// <summary>
        /// 同步发送主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public void PulishOneTopic(string topic, string message) {
            using (var session = poolConnection.CreateSession()) {
                using (IMessageProducer producer = session.CreateProducer(new ActiveMQTopic(topic))) {
                    producer.RequestTimeout = requestTimeout;
                    //可以写入字符串，也可以是一个xml字符串等
                    var req = session.CreateTextMessage(message);
                    producer.Send(req);
                }
            }
        }

        /// <summary>
        /// 发送p2p消息，异步
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        public Task SendP2POneMessageAsync(string queueName, string message) {
            return Task.Run(() => {
                this.SendP2POneMessage(queueName, message);
            });
        }

        /// <summary>
        /// 同步发送P2P消息
        /// </summary>
        /// <param name="queueName">队列名字</param>
        /// <param name="message">内容</param>
        public void SendP2POneMessage(string queueName, string message) {
            using (var session = poolConnection.CreateSession()) {
                IDestination destination = SessionUtil.GetDestination(session, queueName);
                using (IMessageProducer producer = session.CreateProducer(destination)) {
                    producer.RequestTimeout = requestTimeout;
                    ITextMessage request = session.CreateTextMessage(message);
                    producer.Send(request);
                }
            }
        }

        /// <summary>
        /// 监听P2P消息，同步
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="onMessageReceived"></param>
        public void ListenP2PMessage(string queueName, Action<string> onMessageReceived) {
            //注意这里因为有回调，所以不能用 using
            var session = poolConnection.CreateSession();
            IDestination destination = SessionUtil.GetDestination(session, queueName);
            IMessageConsumer consumer = session.CreateConsumer(destination);
            consumer.Listener += new MessageListener((msg) => {
                string text = (msg as ITextMessage)?.Text;
                onMessageReceived.Invoke(text);
            });
        }

        /// <summary>
        /// 监听P2P消息，异步
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="onMessageReceived"></param>
        /// <returns></returns>
        public Task ListenP2PMessageAsync(string queueName, Action<string> onMessageReceived) {
            return Task.Run(() => {
                this.ListenP2PMessage(queueName, onMessageReceived);
            });
        }



        /// <summary>
        /// 监听主题，采用的闭包,同步方法
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="register">注册Id</param>
        /// <param name="selector">持久化消息的唯一 Id</param>
        /// <param name="onMessageReceived">接受事件</param>
        /// <returns></returns>
        public void ListenTopic(string topic, string selector, string register, Action<string> onMessageReceived) {
            selector = "aphard_" + selector;
            ISession session = poolConnection.CreateSession();
            IMessageConsumer consumer = null;
            if (!string.IsNullOrEmpty(register)) {
                consumer = session.CreateDurableConsumer(new ActiveMQTopic(topic), selector, "receiver='" + register + "'", false);
            } else {
                consumer = session.CreateDurableConsumer(new ActiveMQTopic(topic), selector, null, false);
            }
            consumer.Listener += new MessageListener((msg) => {
                ITextMessage message = msg as ITextMessage;
                if (message != null) {
                    onMessageReceived(message.Text);
                }
            });

        }

        public void Close() {
            poolConnection.Close();
        }
    }
}
