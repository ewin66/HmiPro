using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace YCsharp.Util {
    /// <summary>
    /// 反射工具
    /// <date>2017-09-27</date>
    /// <author>ychost</author>
    /// </summary>
    public static partial class YUtil {

        /// <summary>
        /// 给某个类型设置其静态属性值，通过字典
        /// type里面不能有嵌套类型
        /// </summary>
        public static void SetStaticField(Type type, IDictionary<string, object> dict) {
            var typeFilds = GetStaticFieldDict(type);
            foreach (var field in typeFilds) {
                if (dict.ContainsKey(field.Key)) {
                    var val = dict[field.Key];
                    object setVal = val;
                    if (val is Int64 int64Val) {
                        setVal = (int)int64Val;
                    }
                    type.GetField(field.Key).SetValue(null, setVal);
                }
            }
        }


        /// <summary>
        /// 获取类型静态属性和值
        /// {prop:val}
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetStaticFieldDict(Type type) {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(f => f.Name,
                    f => f.GetValue(null));
        }

        /// <summary>
        /// 获取所有静态字段
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<FieldInfo> GetStaticFields(Type type) {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static).ToList();
        }

        /// <summary>
        /// 对配置内容进行校验,目前只检查Require注解
        /// </summary>
        public static void ValidRequiredConfig(Type type) {
            StaticFieldDoAction(type, (field) => {
                var attrs = field.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
                if (attrs.Length > 0) {
                    if (field.GetValue(null) == null) {
                        throw new Exception("配置：" + field.ToString() + " 不能为空");
                    }
                }

            });
        }

        /// <summary>
        /// 对静态成员执行动作
        /// </summary>
        /// <param name="type"></param>
        /// <param name="validAction"></param>
        public static void StaticFieldDoAction(Type type, Action<FieldInfo> validAction) {
            var fields = GetStaticFields(type);
            fields.ForEach(validAction);
        }


        /// <summary>
        /// 获取私属性{含有get;set}的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyname"></param>
        /// <returns></returns>
        public static T GetPrivateProperty<T>(this object instance, string propertyname) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic; Type type = instance.GetType();
            PropertyInfo field = type.GetProperty(propertyname, flag);
            return (T)field.GetValue(instance, null);
        }

        /// <summary>
        /// 调用私有方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(name, flag);
            return (T)method.Invoke(instance, param);
        }

        /// <summary>
        /// 直接调用私有方法，无返回值
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        public static void CallPrivateMethod(this object instance, string name, params object[] param) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(name, flag);
            method.Invoke(instance, param);
        }


        /// <summary>
        /// 直接获取私有方法
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static MethodInfo GetPrivateMethod(this object instance, string name) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(name, flag);
            return method;
        }
        /// <summary>
        /// 获取私有字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(this object instance, string fieldname) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flag);
            return (T)field.GetValue(instance);
        }
        /// <summary>
        /// 获取私有字段指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldname"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(this object instance, string fieldname, Type type) {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = type.GetField(fieldname, flag);
            return (T)field.GetValue(instance);
        }



        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblyName">程序集</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string @namespace, string typename, string assemblyName) {
            string path = @namespace + "." + typename + "," + assemblyName;//命名空间.类型名,程序集
            Type o = Type.GetType(path);//加载类型
            object obj = Activator.CreateInstance(o, true);//根据类型创建实例
            return (T)obj;//类型转换并返回
        }


        /// <summary>
        /// 获取命名空间下的所有类名,仅仅含有类名，不含有空间名字
        /// </summary>
        /// <param name="am"></param>
        /// <param name="namespace"></param>
        /// <param name="containChildSpace">是否包含子空间</param>
        /// <returns></returns>
        public static List<string> GetTypeNameOfNamespace(Assembly am, string @namespace, bool containChildSpace = false) {
            List<string> classList = new List<string>();
            foreach (var type in am.GetTypes()) {
                if (containChildSpace) {
                    if (type.Namespace.StartsWith(@namespace)) {
                        classList.Add(type.Name);
                    }
                } else if (type.Namespace == @namespace) {
                    classList.Add(type.Name);
                }
            }
            return classList;
        }

        /// <summary>
        /// 获取属性的名字
        /// <example>
        ///     var propertyName = GetPropertyName(() => myObject.AProperty); // returns "AProperty"
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression) {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }

        /// <summary>
        /// DataRow获取column的数据
        /// 如果不存在 column 列则返回null
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetValue(this DataRow row, string column) {
            return row.Table.Columns.Contains(column) ? row[column].ToString() : null;
        }

        /// <summary>
        /// 获取某个集合下面的类型
        /// 如果未指明命名空间，则返回所有与之同名的类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<Type> GetTypes(string name, Assembly assembly) {
            var types = new List<Type>();
            foreach (var type in assembly.GetTypes()) {
                if (type.Name == name) {
                    types.Add(type);
                }
            }
            return types;
        }

        /// <summary>
        /// 获取枚举类型的注解值
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();
        }

    }
}
