using Amplitude.Unity.Audio;
using Amplitude.Unity.Framework;
using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ping_Mod
{
    class PingMod : PartialityMod
    {
        ScadMod mod = new ScadMod();

        private Dictionary<GameObject, DateTime> dict = new Dictionary<GameObject, DateTime>();

        public override void Init()
        {
            mod.path = @"Ping_log.txt";
            mod.config = @"Ping_config.txt";
            mod.default_config = "# Modify this file to change various settings of the Ping Mod for DotE.\n" + mod.default_config;
            mod.Initialize();

            // Setup default values for config
            mod.Values.Add("Key", "G");
            mod.Values.Add("Seconds Active", "3");

            mod.ReadConfig();

            mod.Log("Initialized!");
        }
        public override void OnLoad()
        {
            mod.Load();
            if (Convert.ToBoolean(mod.Values["Enabled"]))
            {
                On.Session.Update += Session_Update;
            }
        }

        private void Session_Update(On.Session.orig_Update orig, Session self)
        {
            for (int i = 0; i < dict.Keys.Count; i++)
            {
                // Don't let the user ping a second time.
                orig(self);
                GameObject displayed = dict.Keys.ElementAt(i);
                // Count down the seconds
                if (DateTime.Now.Subtract(dict[displayed]).Seconds >= Convert.ToInt16(mod.Values["Seconds Active"]))
                {
                    // Time is up! Remove the object
                    Dungeon d = SingletonManager.Get<Dungeon>(false);

                    CrystalModuleSlot component = displayed.GetComponent<CrystalModuleSlot>();

                    d.StartRoom.CrystalModuleSlots.Remove(component);

                    OffscreenMarker mark = new DynData<CrystalModuleSlot>(component).Get<OffscreenMarker>("crystalMarker");

                    mark.Hide();
                    UnityEngine.Object.Destroy(mark);
                    UnityEngine.Object.Destroy(displayed);
                    mod.Log("Time is up, destroyed displayed object!");
                    dict.Remove(displayed);
                    i--;
                }
                mod.Log("Waiting till the object is destroyed!");
            }
            KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Key"]);
            if (Input.GetKeyDown(key))
            {
                // First, find the mouse position as a unity vector
                // Next, spawn something there that lasts for x seconds
                // Finally, remove it after those seconds pass
                IGameCameraService gameCameraManager = Services.GetService<IGameCameraService>();

                RaycastHit[] array = Physics.RaycastAll(gameCameraManager.ScreenPointToRay(Input.mousePosition), float.PositiveInfinity);
                Array.Sort<RaycastHit>(array, (RaycastHit hitInfo1, RaycastHit hitInfo2) => hitInfo1.distance.CompareTo(hitInfo2.distance));

                // In theory the first raycast hit should be the best?
                // Not sure if this is the case...
                Vector3 mousePos = array[0].point;

                mod.Log("Raycast hits:");
                foreach (RaycastHit ra in array)
                {
                    mod.Log("Dist: " + ra.distance + " with pos: " + ra.point);
                }

                mod.Log("Mouse pos: "+mousePos.ToString());
                Dungeon d = SingletonManager.Get<Dungeon>(false);

                DynData<Room> r = new DynData<Room>(d.StartRoom);

                GameObject pfb = r.Get<GameObject>("crystalModuleSlotPfb");

                GameObject o = (GameObject)UnityEngine.Object.Instantiate(pfb, mousePos, Quaternion.identity);
                CrystalModuleSlot component = o.GetComponent<CrystalModuleSlot>();

                ulong ownerPlayerID = new DynData<Dungeon>(d).Get<GameNetworkManager>("gameNetManager").GetLocalPlayerID();

                component.Init(ownerPlayerID, d.StartRoom, true);
                d.StartRoom.CrystalModuleSlots.Add(component);
                new DynData<Dungeon>(d).Get<IAudioEventService>("audioEventManager").Play2DEvent("Master/Jingles/Exit");

                dict.Add(o, DateTime.Now);

                mod.Log("Attempting to display!");
            }
            orig(self);
        }
    }
}
