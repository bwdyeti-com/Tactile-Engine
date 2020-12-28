using System.Linq.Expressions;
using System.Reflection;
using System;

namespace Tactile
{
    static class SymbolExtensions
    {
        public static MethodInfo GetMethodInfo<T1, T2>(Action<T1, T2> a)
        {
            return a.Method;
        }
        public static string GetMethodName<T1, T2>(Action<T1, T2> a)
        {
            return GetMethodInfo(a).Name;
        }

        public static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> a)
        {
            return a.Method;
        }
        public static string GetMethodName<T1, T2>(Func<T1, T2> a)
        {
            return GetMethodInfo(a).Name;
        }
    }
}