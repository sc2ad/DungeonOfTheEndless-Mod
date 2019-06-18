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

    }
}
