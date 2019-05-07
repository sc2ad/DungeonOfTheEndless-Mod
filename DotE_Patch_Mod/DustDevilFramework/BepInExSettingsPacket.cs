using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustDevilFramework
{
    internal class BepInExSettingsPacket
    {
        internal object Wrapper { get; }
        internal string Name { get; }
        internal Type Type { get; }
        internal BepInExSettingsPacket(object wrapper, string name, Type type)
        {
            Wrapper = wrapper;
            Name = name;
            Type = type;
        }
    }
}
