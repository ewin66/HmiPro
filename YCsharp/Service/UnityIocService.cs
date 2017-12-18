using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace YCsharp.Service {
    /// <summary>
    /// 基于Unity框架的Ioc服务，处理全局依赖注入
    /// <date>2017-09-30</date>
    /// <author>ychost</author>
    /// </summary>
    public static class UnityIocService {
        public static readonly UnityContainer Container;
        private static readonly IDictionary<Type, bool> injectOnceCheckDictionary;

        static UnityIocService() {
            Container = new UnityContainer();
            injectOnceCheckDictionary = new ConcurrentDictionary<Type, bool>();
        }

        /// <summary>
        /// 注册全局的依赖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterGlobalDepend<T>() {
            Container.RegisterType<T>(new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// 获取全局注入的依赖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ResolveDepend<T>() {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// 注册全局依赖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depend">依赖实例</param>
        public static void RegisterGlobalDepend<T>(T depend) {
            Container.RegisterInstance<T>(depend, new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// 检验是否为第一次注入
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AssertIsFirstInject(Type type) {
            if (!injectOnceCheckDictionary.ContainsKey(type)) {
                injectOnceCheckDictionary[type] = true;
            } else {
                injectOnceCheckDictionary[type] = false;
                throw new Exception(type + "已经被注入了，请勿重复注入此全局依赖");
            }
            return injectOnceCheckDictionary[type];
        }
    }
}
