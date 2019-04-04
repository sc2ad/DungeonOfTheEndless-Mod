using DustDevilFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_Mod
{
    class TemplateSettings : ModSettings
    {
        // Add fields here! Fields that are supported include ints, floats, bools, doubles, strings
        // Other fields can be stored here, but will not be saved to a config file
        // You can preface each field with a flag from DustDevilFramework.ModSettings
        public TemplateSettings(string name) : base(name)
        {
        }
    }
}
