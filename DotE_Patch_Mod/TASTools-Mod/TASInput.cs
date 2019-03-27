using Amplitude.Unity.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TASTools_Mod
{
    class TASInput
    {
        public static TASInput Empty = new TASInput();
        public static Dictionary<int, List<TASInput>> inputs = new Dictionary<int, List<TASInput>>();
        public static Dictionary<int, SeedData> seeds = new Dictionary<int, SeedData>();
        private static IInputService inputManager;
        private static int currentIndex = 0;

        public static bool HasNext(int level)
        {
            if (!inputs.ContainsKey(level))
            {
                currentIndex = 0;
                return false;
            }
            if (currentIndex >= inputs[level].Count)
            {
                currentIndex = 0;
                return false;
            }
            return true;
        }
        public static TASInput GetNext(int level)
        {
            currentIndex++;
            return inputs[level][currentIndex - 1];

        }
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
        private static string GetKeys()
        {
            if (inputManager == null)
            {
                inputManager = Services.GetService<IInputService>();
            }
            string o = "";
            foreach (Control c in Enum.GetValues(typeof(Control)))
            {
                o += inputManager.GetControl(c) ? 1 : 0;
            }
            return o;
        }

        public Vector3 mousePos;
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
        private TASInput()
        {
            // Empty constructor.
            keys = "";
            foreach (Control c in Enum.GetValues(typeof(Control)))
            {
                keys += "0";
            }
            mousePos = new Vector3(0, 0, 0);
            Button0 = false;
            Button1 = false;
            Button2 = false;
        }
        public TASInput(int level)
        {
            mousePos = Input.mousePosition;
            Button0 = Input.GetMouseButton(0);
            Button1 = Input.GetMouseButton(1);
            Button2 = Input.GetMouseButton(2);
            keys = GetKeys();
            add(level);
        }
        public TASInput(int level, string s)
        {
            string[] spl = s.Split(';');
            if (spl.Length != 5)
            {
                // UHOH!
                throw new Exception("SPL INVALID: " + spl);
            }
            string[] vect = spl[0].Split('(')[1].Split(')')[0].Split(new string[] { ", " }, StringSplitOptions.None);
            mousePos = new Vector3((float) Convert.ToDouble(vect[0]), (float) Convert.ToDouble(vect[1]), (float) Convert.ToDouble(vect[2]));
            Button0 = Convert.ToBoolean(spl[1]);
            Button1 = Convert.ToBoolean(spl[2]);
            Button2 = Convert.ToBoolean(spl[3]);
            keys = spl[4];
            add(level);
        }
        public bool GetControl(Control c)
        {
            int index = 0;
            foreach (Control item in Enum.GetValues(typeof(Control)))
            {
                if (item.Equals(c))
                {
                    // The item matches!
                    return keys[index].Equals('1');
                }
                index++;
            }
            // Should never get here!
            return false;
        }
        public override string ToString()
        {
            return mousePos.ToString() + ';' + Button0 + ';' + Button1 + ';' + Button2 + ';' + keys;
        }
    }
}
