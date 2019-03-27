using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TASTools_Mod
{
    class TASInputPlayer
    {
        public static TASInput Current;
        public static TASInput Last;

        public static void Update(TASInput next)
        {
            Last = Current;
            Current = next;
            if (Last == null)
            {
                Last = TASInput.Empty;
            }
        }

        public static Vector3 GetMousePos()
        {
            return Current.mousePos;
        }

        public static bool GetMouseButtonDown(int button)
        {
            switch (button)
            {
                case 0:
                    return !Last.Button0 && Current.Button0;
                case 1:
                    return !Last.Button1 && Current.Button1;
                case 2:
                    return !Last.Button2 && Current.Button2;
            }
            return false;
        }
        public static bool GetMouseButtonUp(int button)
        {
            switch (button)
            {
                case 0:
                    return Last.Button0 && !Current.Button0;
                case 1:
                    return Last.Button1 && !Current.Button1;
                case 2:
                    return Last.Button2 && !Current.Button2;
            }
            return false;
        }
        public static bool GetControlStatus(Control control, ControlStatus status)
        {
            switch (status)
            {
                case ControlStatus.JustDown:
                    return !Last.GetControl(control) && Current.GetControl(control);
                case ControlStatus.JustUp:
                    return Last.GetControl(control) && !Current.GetControl(control);
                case ControlStatus.CurrentlyDown:
                default:
                    return Current.GetControl(control);
            }
        }
    }
}
