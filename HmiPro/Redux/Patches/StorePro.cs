using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;
using Reducto;
using YCsharp.Util;

namespace HmiPro.Redux.Patches {
    /// <summary>
    /// 给store打的补丁
    /// [x] Dispatch IAction method for specify action type
    /// [x] Dispatch async action dispatch delegate fixed
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorePro<T> : Store<T> {

        private object baseStore;
        readonly object dispatchLock = new object();
        public StorePro(SimpleReducer<T> rootReducer) : base(rootReducer) {
            init();
        }

        public StorePro(CompositeReducer<T> rootReducer) : base(rootReducer) {
            init();
        }

        public StorePro(Reducer<T> rootReducer) : base(rootReducer) {
            init();
        }

        //== 一些父类的私有元素
        private Reducer<T> rootReducer;
        private dynamic state;
        private List<StateChangedSubscriber<T>> subscriptions;
        private MiddlewareExecutor middlewares;

        /// <summary>
        /// 派遣动作的时候将动作名称赋予状态
        /// </summary>
        /// <param name="action"></param>
        public void Dispatch(IAction action) {
            state = rootReducer(state, action);

            //State中必须含有Type字段
            //可能会多个线程同时dispatch，导致state发生不可预见的错误，这里给其加锁
            lock (dispatchLock) {
                state.Type = action.Type();
                foreach (var subscribtion in subscriptions) {
                    subscribtion(state);
                }

            }
        }

        /// <summary>
        /// 强化action定义
        /// </summary>
        /// <param name="action"></param>
        public new void Dispatch(Object action) {
            //middlewares(action);
            if (action is IAction defAction) {
                Dispatch(defAction);
            } else {
                throw new Exception($"派遣的action未继承自 IAction");
            }
        }

        public new T GetState() {
            return state;
        }
        /// <summary>
        /// 派遣异步action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public new Task Dispatch(AsyncAction action) {
            return action(Dispatch, GetState);
        }

        /// <summary>
        /// 首次订阅的时候直接返回状态
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public new Unsubscribe Subscribe(StateChangedSubscriber<T> subscription) {
            subscriptions.Add(subscription);
            //立即返回存储的状态
            subscription(state);
            return () => { subscriptions.Remove(subscription); };
        }

        /// <summary>
        /// 初始化继承一些父类的私有元素
        /// </summary>
        private void init() {
            if (baseStore == null) {
                baseStore = this.GetPrivateField<object>("store", GetType().BaseType);
            }
            if (rootReducer == null) {
                rootReducer = baseStore.GetPrivateField<Reducer<T>>("rootReducer");
            }
            if (state == null) {
                state = baseStore.GetPrivateField<T>("state");
            }
            if (subscriptions == null) {
                subscriptions = baseStore.GetPrivateField<List<StateChangedSubscriber<T>>>("subscriptions");
            }
            if (middlewares == null) {
                middlewares = this.GetPrivateField<MiddlewareExecutor>("middlewares");

            }
        }
    }
}
