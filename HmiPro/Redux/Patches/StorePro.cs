using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;
using Reducto;
using YCsharp.Service;
using YCsharp.Util;

namespace HmiPro.Redux.Patches {
    /// <summary>
    /// 给store打的补丁
    /// [x] Dispatch IAction method for specify action type
    /// [x] Dispatch async action dispatch delegate fixed
    /// [x] Subscribe add action param
    /// <date>2017-12-18</date>
    /// <author>ychost</author>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorePro<T> : Store<T> {

        private object baseStore;
        readonly object storeLock = new object();
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
        //只能收到state

        private event StateChangedSubscriber<T> subscriptions;
        //和subscriptions的区别就是可以收到当前的state和action数据
        private event Action<T, IAction> listeners;
        private MiddlewareExecutor middlewares;
        private IAction latestAction;

        /// <summary>
        /// 派遣动作的时候将动作名称赋予状态
        /// </summary>
        /// <param name="action"></param>
        public void Dispatch(IAction action) {
            //注意：state和action都是struct,这里的是赋值不是引用
            //防止在subscription中更改state
            //State中必须含有Type字段
            //可能会多个线程同时dispatch，导致state发生不可预见的错误，这里给其加锁
            lock (storeLock) {
                state = rootReducer(state, action);
                latestAction = action;
                state.Type = action.Type();
                subscriptions?.Invoke(state);
                listeners?.Invoke(state, latestAction);
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

        public new Task<Result> Dispatch<Result>(AsyncAction<Result> action) {
            return action(Dispatch, GetState);
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
            lock (storeLock) {
                subscriptions += subscription;
                //立即返回存储的状态
                if (state.Type != null) {
                    subscription(state);
                }
                return () => { subscriptions -= subscription; };
            }
        }

        public Unsubscribe Subscribe(Action<T, IAction> listener) {
            lock (storeLock) {
                listeners += listener;
                if (state.Type != null && latestAction != null) {
                    listener(state, latestAction);
                }
                return () => {
                    listeners -= listener;
                };
            }
        }


        /// <summary>
        /// 初始化继承一些父类的私有元素
        /// </summary>
        private void init() {
            UnityIocService.AssertIsFirstInject(GetType());
            if (baseStore == null) {
                baseStore = this.GetPrivateField<object>("store", GetType().BaseType);
            }
            if (rootReducer == null) {
                rootReducer = baseStore.GetPrivateField<Reducer<T>>("rootReducer");
            }
            if (state == null) {
                state = baseStore.GetPrivateField<T>("state");
            }
            if (middlewares == null) {
                middlewares = this.GetPrivateField<MiddlewareExecutor>("middlewares");
            }
        }
    }
}
