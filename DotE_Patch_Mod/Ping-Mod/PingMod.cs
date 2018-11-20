using Amplitude.Unity.Audio;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Messaging;
using DustDevilFramework;
using MonoMod.Utils;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Amplitude.Interop.Steamworks.SteamNetworking;

namespace Ping_Mod
{
    class PingMod : PartialityMod
    {
        public static ScadMod mod = new ScadMod();

        private static short PING_ID = 17346;

        public override void Init()
        {
            mod.name = "Ping";
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
                On.InputManager.Update += InputManager_Update;
            }
        }

        private void InputManager_Update(On.InputManager.orig_Update orig, InputManager self)
        {   
            KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), mod.Values["Key"]);
            if (Input.GetKeyDown(key))
            {
                // First, find the mouse position as a unity vector
                // Next, spawn something there that lasts for x seconds
                // Finally, remove it after those seconds pass
                IGameCameraService gameCameraManager = Services.GetService<IGameCameraService>();
                GameNetworkManager net = GameObject.FindObjectOfType<GameNetworkManager>();
                IMessageBox msgBox = new DynData<GameNetworkManager>(net).Get<IMessageBox>("messageBox");

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

                mod.Log("Mouse pos: " + mousePos.ToString());

                bool assert = (bool)typeof(GameNetworkManager).GetMethod("AssertMessageBoxIsSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(net, new object[0]);
                if (assert && msgBox != null && net != null)
                {
                    PingMessage pm = new PingMessage();
                    pm.SetPos(mousePos);
                    Message m = pm as Message;

                    mod.Log("Assertion: " + assert);
                    mod.Log("Net: " + net.name);
                    mod.Log("MSG BOX: " + msgBox.ToString());

                    EP2PSend temp = msgBox.SteamNetworkingSendMode;
                    msgBox.SteamNetworkingSendMode = EP2PSend.k_EP2PSendReliable;
                    msgBox.SendMessage(ref m, net.GetLobbyPlayerIDs());
                    msgBox.SteamNetworkingSendMode = temp;

                    mod.Log("Attempting to display!");
                } else
                {
                    GameObject obj = new GameObject("PING OBJECT");
                    PingScript p = obj.AddComponent<PingScript>();
                    p.pos = mousePos;
                    mod.Log("It seems the game is not in multiplayer, attempting to create a local ping!");
                }
            }
            orig(self);
        }

        class PingScript : MonoBehaviour
        {
            float lifetime;
            public Vector3 pos;
            OffscreenMarker mark;

            public void Start()
            {
                lifetime = (float)Convert.ToDouble(mod.Values["Seconds Active"]);
            }
            public void Update()
            {
                try
                {
                    lifetime -= Time.deltaTime;
                    transform.position = pos;

                    if (mark == null)
                    {
                        mark = SingletonManager.Get<Dungeon>(false).DisplayCrystalAndExitOffscreenMarkers(gameObject.transform);
                    }
                    if (lifetime <= 0)
                    {
                        mark.Hide();
                        Destroy(mark);
                        Destroy(gameObject);
                    }
                } catch (Exception e)
                {
                    if (mark != null)
                    {
                        mark.Hide();
                        Destroy(mark);
                    }
                    Destroy(gameObject);
                }
            }
        }

        class PingMessage : Message
        {
            Vector3 pos;
            public PingMessage() : base(PING_ID) {}
            public void SetPos(Vector3 pos)
            {
                this.pos = pos;
            }
            protected override void Pack(BinaryWriter writer)
            {
                base.Pack(writer);

                CreatePingObject();

                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write(pos.z);

            }
            protected override void Unpack(BinaryReader reader)
            {
                base.Unpack(reader);

                pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                CreatePingObject();
            }
            public void CreatePingObject()
            {
                mod.Log("Creating a ping object!");
                Dungeon d = SingletonManager.Get<Dungeon>(false);
                GameObject obj = new GameObject("PING OBJECT");
                PingScript ping = obj.AddComponent<PingScript>();
                ping.pos = pos;
                new DynData<Dungeon>(d).Get<IAudioEventService>("audioEventManager").Play2DEvent("Master/Jingles/Exit");
            }
        }
    }
}
