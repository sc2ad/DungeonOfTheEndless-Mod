using Amplitude.Unity.Audio;
using Amplitude.Unity.Framework;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;

namespace DuplicateHeroSelection_Mod
{
    [BepInPlugin("com.sc2ad.DuplicateHeroSelection", "Duplicate Hero Selection", "1.0.0")]
    public class DuplicateHeroSelectionMod : BaseUnityPlugin
    {
        private ScadMod mod;
        private ConfigWrapper<int> allowedDuplicateCountWrapper;
        public void Awake()
        {
            mod = new ScadMod("DuplicateHeroSelection", this);

            allowedDuplicateCountWrapper = Config.Wrap<int>("Settings", "AllowedDuplicateCount", "The number of duplicate heroes allowed.", 4);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.HeroSelectionItem.OnLeftClick += HeroSelectionItem_OnLeftClick;
            }
        }

        public void UnLoad()
        {
            // Need to have a way of unloading entirety of the mod from only ScadMod (also deals with unloading partialitymod)
            mod.UnLoad();
            On.HeroSelectionItem.OnLeftClick -= HeroSelectionItem_OnLeftClick;
        }

        private void HeroSelectionItem_OnLeftClick(On.HeroSelectionItem.orig_OnLeftClick orig, HeroSelectionItem self)
        {
            self.GetComponent<AgeControlToggle>().OnRightClickMethod = "OnRightClick";
            try
            {
                self.gameObject.AddComponent<RightClickScript>();
            }
            catch (Exception)
            {
                mod.Log("Already added RightClickScript!");
            }
            // Even if the hero has been selected, try to select it again.
            int duplicates = 0;
            foreach (Amplitude.StaticString s in Dungeon.NextDungeonGenerationParams.NewGame_SelectedHeroes)
            {
                if (s.Equals(self.HeroStats.ConfigName))
                {
                    duplicates++;
                }
            }
            if (duplicates < allowedDuplicateCountWrapper.Value)
            {
                // Try selecting it again!
                mod.Log("Selecting Hero: " + self.HeroStats.ConfigName + " again!");
                IAudioEventService service = Services.GetService<IAudioEventService>();
                SingletonManager.Get<GameSelectionPanel>(true).TrySelectHero(self.HeroStats.ConfigName);
                service.Play2DEvent("Master/GUI/Main_SelectHero");
                return;
            }
            
            orig(self);
        }

        private class RightClickScript : MonoBehaviour
        {
            public void OnRightClick()
            {
                SingletonManager.Get<GameSelectionPanel>(true).TryUnselectHero(GetComponent<HeroSelectionItem>().HeroStats.ConfigName);
                IAudioEventService service = Services.GetService<IAudioEventService>();
                service.Play2DEvent("Master/GUI/Main_DeselectHero");
            }
        }
    }
}
