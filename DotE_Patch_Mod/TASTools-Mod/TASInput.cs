using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASTools_Mod
{
    class TASInput
    {
        public static Dictionary<int, List<TASInput>> inputs = new Dictionary<int, List<TASInput>>();
        public static Dictionary<int, SeedData> seeds = new Dictionary<int, SeedData>();

        public static void CreateAndAdd(int level)
        {
            new TASInput(level);
        }
        public static void AddSeed(int level, SeedData data)
        {
            if (!seeds.ContainsKey(level))
            {
                seeds.Add(level, data);
            }
        }
        public static void Clear()
        {
            inputs = new Dictionary<int, List<TASInput>>();
            seeds = new Dictionary<int, SeedData>();
        }
        public static void ClearForLevel(int level)
        {
            inputs[level] = null;
            seeds[level] = null;
        }

        public string input;
        public UnityEngine.Vector3 mousePos;
        public bool Button0;
        public bool Button1;
        public bool Button2;
        public string keys;
        private void add(int level)
        {
            if (!inputs.ContainsKey(level))
            {
                inputs.Add(level, new List<TASInput>());
            }
            inputs[level].Add(this);
        }
        public TASInput(int level)
        {
            mousePos = UnityEngine.Input.mousePosition;
            Button0 = UnityEngine.Input.GetMouseButton(0);
            Button1 = UnityEngine.Input.GetMouseButton(1);
            Button2 = UnityEngine.Input.GetMouseButton(2);
            keys = UnityEngine.Input.inputString;
            input = mousePos.ToString() + ';' + Button0 + ';' + Button1 + ';' + Button2 + ';' + keys;
            add(level);
        }
        public TASInput(int level, string s)
        {
            input = s;
            string[] spl = s.Split(';');
            if (spl.Length != 5)
            {
                // UHOH!
                throw new Exception("SPL INVALID: " + spl);
            }
            string[] vect = spl[0].Split('(')[1].Split(')')[0].Split(new string[] { ", " }, StringSplitOptions.None);
            mousePos = new UnityEngine.Vector3((float) Convert.ToDouble(vect[0]), (float) Convert.ToDouble(vect[1]), (float) Convert.ToDouble(vect[2]));
            Button0 = Convert.ToBoolean(spl[1]);
            Button1 = Convert.ToBoolean(spl[2]);
            Button2 = Convert.ToBoolean(spl[3]);
            keys = spl[4];
            add(level);
        }
        public override string ToString()
        {
            return input;
        }
    }
}
