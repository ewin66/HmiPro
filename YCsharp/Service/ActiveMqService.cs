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
    public class ActiveMqService {
        private string mqConn;
        private string mqUserName;
        private string mqUserPwd;

        public ActiveMqService(string mqConn, string mqUserName, string mqUserPwd) {
            this.mqConn = mqConn;
            this.mqUserName = mqUserName;
            this.mqUserPwd = mqUserPwd;
        }
        /// <summary>
        /// 基础操作,只能执行一次动作
        /// </summary>
        public void OperateOneAction(Action<ISession, IConnection> action, bool autoDispose = true) {
            Uri uri = new Uri(mqConn);
            IConnectionFactory factory = new ConnectionFactory(uri);
            if (autoDispose) {
                using (IConnection conn = factory.CreateConnection(mqUserName, mqUserPwd)) {
                    using (ISession session = conn.CreateSession()) {
                        action(session, conn);
                    }
                }
            } else {
                IConnection conn = factory.CreateConnection(mqUserName, mqUserPwd);
                ISession session = conn.CreateSession();
                action(session, conn);
            }
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
            this.OperateOneAction((session, conn) => {
                using (IMessageProducer producer = session.CreateProducer(new ActiveMQTopic(topic))) {
                    conn.Start();
                    //可以写入字符串，也可以是一个xml字符串等
                    var req = session.CreateTextMessage(message);
                    producer.Send(req);
                }
            });
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
            this.OperateOneAction((session, conn) => {
                IDestination destination = SessionUtil.GetDestination(session, queueName);
                using (IMessageProducer producer = session.CreateProducer(destination)) {
                    conn.Start();
                    ITextMessage request = session.CreateTextMessage(message);
                    producer.Send(request);
                }
            });
        }

        /// <summary>
        /// 监听P2P消息，同步
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="onMessageReceived"></param>
        public void ListenP2PMessage(string queueName, Action<string> onMessageReceived) {
            this.OperateOneAction((session, conn) => {
                conn.Start();
                IDestination destination = SessionUtil.GetDestination(session, queueName);
                IMessageConsumer consumer = session.CreateConsumer(destination);
                consumer.Listener += new MessageListener((msg) => {
                    string text = (msg as ITextMessage)?.Text;
                    onMessageReceived.Invoke(text);
                });

            }, false);
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
        /// 监听主题，异步方法
        /// </summary>
        public Task ListenTopicAsync(string topic, string register, Action<string> onMessageReceived) {
            return Task.Run(() => {
                this.ListenTopic(topic, register, onMessageReceived)();
            });
        }

        /// <summary>
        /// 监听主题，采用的闭包,同步方法
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="register">注册Id</param>
        /// <param name="onMessageReceived">接受事件</param>
        /// <returns></returns>
        public Action ListenTopic(string topic, string register, Action<string> onMessageReceived) {
            Uri uri = new Uri(mqConn);
            IConnectionFactory factory = new ConnectionFactory(uri);
            IConnection conn = factory.CreateConnection(mqUserName, mqUserPwd);
            if (!string.IsNullOrEmpty(register)) {
                conn.ClientId = register + YUtil.GetUtcTimestampMs(DateTime.Now);
            }
            conn.Start();
            return () => {
                ISession session = conn.CreateSession();
                IMessageConsumer consumer = null;
                if (!string.IsNullOrEmpty(register)) {
                    consumer = session.CreateDurableConsumer(new ActiveMQTopic(topic), register,
                        "receiver='" + register + "'", false);
                } else {
                    consumer = session.CreateDurableConsumer(new ActiveMQTopic(topic), "csharp", null, false);
                }
                consumer.Listener += new MessageListener((msg) => {
                    ITextMessage message = msg as ITextMessage;
                    if (message != null) {
                        onMessageReceived(message.Text);
                    }
                });
            };

        }
    }
}
