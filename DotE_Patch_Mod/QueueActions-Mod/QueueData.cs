using Amplitude.Unity.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace QueueActions_Mod
{
    public class QueueData
    {
        public ClickDownInfo info;

        public Hero hero;

        public NPCMerchant merch;

        public Room room;

        public Door door;

        public CrystalPanel crystalPanel;
        public MajorModule module;

        public void Setup(ClickDownInfo info)
        {
            this.info = info;
        }

        public QueueData(Hero hero, ClickDownInfo info)
        {
            this.hero = hero;
            Setup(info);
        }
        public QueueData(NPCMerchant merch, ClickDownInfo info)
        {
            this.merch = merch;
            Setup(info);
        }
        public QueueData(Room room, ClickDownInfo info)
        {
            this.room = room;
            Setup(info);
        }
        public QueueData(Door door, ClickDownInfo info)
        {
            this.door = door;
            Setup(info);
        }
        public QueueData(CrystalPanel panel, MajorModule module)
        {
            crystalPanel = panel;
            this.module = module;
        }
        public void CallOrig()
        {
            if (hero != null)
            {
                Debug.Log("Hero Orig");
                //heroOrig(hero, info);
            }
            else if (merch != null)
            {
                Debug.Log("Merch Orig");
                //merchOrig(merch, info);
            }
            else if (room != null)
            {
                Debug.Log("Room Orig");
                //roomOrig(room, info);
                // Invokes the original OnRightClickDown
                //method.Invoke(room, new object[] { info });
                room.MoveSelectedHeroesToRoom();
            }
            else if (door != null)
            {
                Debug.Log("Door Orig");
                // This could be replaced with orig for Door.OnRightClickDown
                foreach (Hero hero in Hero.SelectedHeroes)
                {
                    hero.MoveToDoor(door, false, null, true);
                }
            }
            else if (crystalPanel != null)
            {
                Debug.Log("Crystal Orig");
                foreach (Hero hero in Hero.SelectedHeroes)
                {
                    hero.MoveToCrystal(module);
                }
                crystalPanel.Hide(true);
            }
            Services.GetService<IInputService>().StopClickEventPropagation();
        }
    }
}
