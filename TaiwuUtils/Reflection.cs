using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TaiwuUtils
{
    public static class Reflection
    {
        const BindingFlags flags = BindingFlags.Instance
                                 | BindingFlags.Static
                                 | BindingFlags.NonPublic
                                 | BindingFlags.Public;

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="instance">实例（若静态方法则为null)</param>
        /// <param name="method">方法名称</param>
        /// <param name="args">参数</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object Invoke<T>(T instance, string method, params object[] args)
        {
            return Invoke(typeof(T), instance, method, args);
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="instance">实例（若静态方法则为null)</param>
        /// <param name="method">方法名称</param>
        /// <param name="argTypes">参数类型</param>
        /// <param name="args">参数</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object Invoke<T>(T instance, string method, Type[] argTypes, params object[] args)
        {
            return Invoke(typeof(T), instance, method, argTypes, args);
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="method">方法名称</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static object Invoke(Type type, object instance, string method, params object[] args)
        {
            return type.GetMethod(method, flags).Invoke(instance, args);
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="method">方法名称</param>
        /// <param name="argTypes">参数类型</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static object Invoke(Type type, object instance, string method, Type[] argTypes, params object[] args)
        {
            argTypes = argTypes ?? new Type[0];

            var methods = type.GetMethods(flags).Where(m =>
            {
                if (m.Name != method)
                    return false;
                return m.GetParameters()
                        .Select(p => p.ParameterType)
                        .SequenceEqual(argTypes);
            });

            if (methods.Count() != 1)
            {
                throw new AmbiguousMatchException();
            }

            return methods.First().Invoke(instance, args);
        }

        /// <summary>
        /// 获取字段的值
        /// </summary>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="field">字段名称</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetField<T>(T instance, string field)
        {
            return GetField(typeof(T), instance, field);
        }

        /// <summary>
        /// 获取字段的值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="field">字段名称</param>
        /// <returns></returns>
        public static object GetField(Type type, object instance, string field)
        {
            return type.GetField(field, flags).GetValue(instance);
        }

        /// <summary>
        /// 设置字段的值
        /// </summary>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="field">字段名称</param>
        /// <param name="value">值</param>
        /// <typeparam name="T"></typeparam>
        public static void SetField<T>(T instance, string field, object value)
        {
            SetField(typeof(T), instance, field, value);
        }

        /// <summary>
        /// 设置字段的值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例（若静态字段则为null)</param>
        /// <param name="field">字段名称</param>
        /// <param name="value">值</param>
        public static void SetField(Type type, object instance, string field, object value)
        {
            type.GetField(field, flags).SetValue(instance, value);
        }
    }
}