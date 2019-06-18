using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DustDevilFramework
{
    public static class ReflectionUtil
    {
        public static PropertyInfo GetPrivateProperty(this Type type, string name)
        {
            return type.GetProperty(name, BindingFlags.NonPublic);
        }
        public static V GetField<T, V>(this object c, string name) where T : class
        {
            return new DynData<T>((T)c).Get<V>(name);
        }
        public static void SetField<T, V>(this object c, string name, V item) where T : class
        {
            new DynData<T>((T)c).Set(name, item);
        }
    }
}
