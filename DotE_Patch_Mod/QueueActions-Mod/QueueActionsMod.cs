using Amplitude.Unity.Framework;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace QueueActions_Mod
{
    [BepInPlugin("com.sc2ad.QueueActions", "QueueActions", "1.0.0")]
    class QueueActionsMod : BaseUnityPlugin
    {
        ScadMod mod;

        //List<QueueData> queue = new List<QueueData>();
        // Each Hero needs to have their own queue.
        Dictionary<Hero, List<QueueData>> queue = new Dictionary<Hero, List<QueueData>>();

        private ConfigWrapper<string> keyWrapper;

        public void Awake()
        {
            mod = new ScadMod("QueueActions", typeof(QueueActionsMod), this);

            keyWrapper = Config.Wrap<string>("Settings", "Key", "The UnityEngine.KeyCode used to queue up actions.", KeyCode.LeftShift.ToString());

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.Hero.OnRightClickDown += Hero_OnRightClickDown;
                On.NPCMerchant.OnRightClickDown += NPCMerchant_OnRightClickDown;
                On.Room.OnRightClickDown += Room_OnRightClickDown;
                On.Door.OnRightClickDown += Door_OnRightClickDown;
                On.RoomTacticalMapElement.OnRightClickDown += RoomTacticalMapElement_OnRightClickDown;
                On.MajorModule.OnRightClickDown += MajorModule_OnRightClickDown;
                On.CrystalPanel.OnUnplugButtonClicked += CrystalPanel_OnUnplugButtonClicked;
                On.InputManager.Update += InputManager_Update;
                On.Hero.OnBlockingDoorOpened += Hero_OnBlockingDoorOpened;
            }
        }
        public void UnLoad()
        {
            mod.UnLoad();
            On.Hero.OnRightClickDown -= Hero_OnRightClickDown;
            On.NPCMerchant.OnRightClickDown -= NPCMerchant_OnRightClickDown;
            On.Room.OnRightClickDown -= Room_OnRightClickDown;
            On.Door.OnRightClickDown -= Door_OnRightClickDown;
            On.RoomTacticalMapElement.OnRightClickDown -= RoomTacticalMapElement_OnRightClickDown;
            On.MajorModule.OnRightClickDown -= MajorModule_OnRightClickDown;
            On.CrystalPanel.OnUnplugButtonClicked -= CrystalPanel_OnUnplugButtonClicked;
            On.InputManager.Update -= InputManager_Update;
            On.Hero.OnBlockingDoorOpened -= Hero_OnBlockingDoorOpened;
        }

        public void PopQueue(Hero h, bool remove = true)
        {
            // Then need to apply the action at the top of the queue (which should be index 0)
            // To do this, first we need to set the selected heroes to be the hero that has the action in the queue
            List<Hero> prevSelected = Hero.SelectedHeroes;
            var field = typeof(Hero).GetField("selectedHeroes", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            // Now call the original action from the queue
            mod.Log("Setting Selected Heroes");
            field.SetValue(null, new List<Hero>() { h });
            mod.Log("Attempting to call QueueCallOrig!");
            if (queue[h][0] == null)
            {
                mod.Log("QueueData is null!");
            }
            queue[h][0].CallOrig();
            // Reset the selected heroes
            mod.Log("Setting Previous Heroes back to Selected!");
            field.SetValue(null, prevSelected);
            // Remove the action so that it isn't called again
            if (remove)
            {
                queue[h].RemoveAt(0);
            }
            mod.Log("Hero with name: " + h.LocalizedName + " is now moving using their most recent Queued Action, with: " + queue[h].Count + " actions in their queue!");
        }

        private void Hero_OnBlockingDoorOpened(On.Hero.orig_OnBlockingDoorOpened orig, Hero self)
        {
            // This handles the case when the queue has items immediately after opening a door!
            orig(self);
            if (queue.ContainsKey(self))
            {
                if (queue[self] != null && queue[self].Count > 0)
                {
                    // The queue has items! AND we just finished opening the door.
                    // Instead of moving into the room/door, let us instead pop from the queue
                    mod.Log("Popping from queue because the door has been opened!");
                    // I actually need to make sure that I don't remove the action (because then it actually gets removed twice)

                    PopQueue(self, false);
                }
            }
        }

        private void InputManager_Update(On.InputManager.orig_Update orig, InputManager self)
        {
            orig(self);
            List<Hero> localHeroes = Hero.LocalPlayerActiveRecruitedHeroes;
            foreach (Hero h in localHeroes)
            {
                // First need to confirm that the hero is done with all of their current actions so that the next action can be accomplished
                if (!h.MoverCpnt.IsMoving && h.MoverCpnt.CanMove && h.IsUsable && new DynData<Hero>(h).Get<Item>("gatheringItem") == null) // or need to check to see if i just opened a door, cause then i can pop from the queue.
                {
                    // Then need to confirm that the hero has a queue
                    if (queue.ContainsKey(h))
                    {
                        if (queue[h] == null)
                        {
                            mod.Log("Null hero list... This should never happen!");
                            continue;
                        }
                        if (queue[h].Count == 0)
                        {
                            // There are no actions in the queue
                            continue;
                        }
                        PopQueue(h);
                    }
                }
            }
        }

        private bool ConditionalAddToQueue(QueueData data)
        {
            KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), keyWrapper.Value);
            if (Input.GetKey(key))
            {
                mod.Log("ShiftKey Down!");
                foreach (Hero h in Hero.SelectedHeroes)
                {
                    if (!queue.ContainsKey(h))
                    {
                        queue.Add(h, new List<QueueData>());
                    }
                    List<QueueData> personalQueue = queue[h];
                    personalQueue.Add(data);
                    mod.Log("Added action to hero with name: " + h.LocalizedName + "'s queue!");
                }
                return true;
            } else
            {
                // Remove everything from this Hero's queue!
                foreach (Hero h in Hero.SelectedHeroes)
                {
                    if (queue.ContainsKey(h))
                    {
                        queue[h].Clear();
                    }
                    mod.Log("Removed all queued actions for hero with name: " + h.LocalizedName);
                }
            }
            return false;
            // Possibly need to add Services.GetService<IInputService>().StopClickEventPropagation();
        }

        private void NPCMerchant_OnRightClickDown(On.NPCMerchant.orig_OnRightClickDown orig, NPCMerchant self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(self, clickInfo)))
            {
                return;
            }
            orig(self, clickInfo);
        }

        private void Hero_OnRightClickDown(On.Hero.orig_OnRightClickDown orig, Hero self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(self, clickInfo)))
            {
                return;
            }
            orig(self, clickInfo);
        }

        private void Room_OnRightClickDown(On.Room.orig_OnRightClickDown orig, Room self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(self, clickInfo)))
            {
                return;
            }
            orig(self, clickInfo);
        }

        private void Door_OnRightClickDown(On.Door.orig_OnRightClickDown orig, Door self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(self, clickInfo)))
            {
                return;
            }
            orig(self, clickInfo);
        }

        private void CrystalPanel_OnUnplugButtonClicked(On.CrystalPanel.orig_OnUnplugButtonClicked orig, CrystalPanel self)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(self, new DynData<CrystalPanel>(self).Get<MajorModule>("module"))))
            {
                self.Hide(false);
                return;
            }
            orig(self);
        }

        private void MajorModule_OnRightClickDown(On.MajorModule.orig_OnRightClickDown orig, MajorModule self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            // Need to make sure that if it is a crystal, the orig is still called.
            // Only call this if it isn't a crystal!
            if (!self.IsCrystal)
            {
                if (ConditionalAddToQueue(new QueueData(self.RoomElement.ParentRoom, clickInfo)))
                {
                    return;
                }
            }
            orig(self, clickInfo);
        }

        private void RoomTacticalMapElement_OnRightClickDown(On.RoomTacticalMapElement.orig_OnRightClickDown orig, RoomTacticalMapElement self, ClickDownInfo clickInfo)
        {
            // If the shift key is down, instead of doing it right away, add it to a queue.
            if (ConditionalAddToQueue(new QueueData(new DynData<RoomTacticalMapElement>(self).Get<Room>("room"), clickInfo)))
            {
                return;
            }
            orig(self, clickInfo);
        }
    }
}
