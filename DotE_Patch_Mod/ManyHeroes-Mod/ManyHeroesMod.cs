using BepInEx;
using BepInEx.Configuration;
using DustDevilFramework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManyHeroes_Mod
{
    [BepInPlugin("com.sc2ad.ManyHeroes", "Many Heroes", "1.0.0")]
    public class ManyHeroesMod : BaseUnityPlugin
    {
        private ScadMod mod;

        private ConfigWrapper<int> maxHeroCountWrapper;
        private ConfigWrapper<int> maxHeroShipCountWrapper;

        private bool run;
        private bool runInMovie;

        public void Awake()
        {
            mod = new ScadMod("ManyHeroes", this);

            // Wrap Settings here!
            maxHeroCountWrapper = Config.Wrap("Settings", "MaxHeroCount", "Maximum heroes ever allowed.", 10);
            maxHeroShipCountWrapper = Config.Wrap("Settings", "MaxHeroShipCount", "Maximum number of heroes allowed for a selected ship.", 4);

            Config.Wrap("SettingsIgnore", "MaxHeroCountMin", defaultValue: 4);
            Config.Wrap("SettingsIgnore", "MaxHeroCountMax", defaultValue: 100);
            Config.Wrap("SettingsIgnore", "MaxHeroShipCountMin", defaultValue: 1);
            Config.Wrap("SettingsIgnore", "MaxHeroShipCountMax", defaultValue: 100);

            mod.Initialize();

            OnLoad();
        }
        public void OnLoad()
        {
            mod.Load();
            if (mod.EnabledWrapper.Value)
            {
                On.MoviePanel.OnMoviePlaybackComplete += MoviePanel_OnMoviePlaybackComplete;
                On.Lift.Show += Lift_Show;
                On.GameSelectionPanel.Display += GameSelectionPanel_Display;
                On.GameSelectionPanel.RefreshContent += GameSelectionPanel_RefreshContent;
                // Add hooks here!
            }
        }



        private void Lift_Show(On.Lift.orig_Show orig, Lift self)
        {
            SetLiftHeroes(self);
            orig(self);
        }

        private void GameSelectionPanel_RefreshContent(On.GameSelectionPanel.orig_RefreshContent orig, GameSelectionPanel self)
        {
            if (!run)
            {
                // Set GameConfig
                SetGameConfig();

                // Need to AT LEAST alter the following:
                // GameSelectionPanel.Display --> NullReference
                // Lift.Show() --> IndexOutOfBounds

                // GameSelectionPanel.Display --> NullReference
                var d = new DynData<GameSelectionPanel>(self);
                mod.Log("Before set:");
                mod.Log("GameSelectionPanel.maxHeroCount: " + d.Get<int>("maxHeroCount"));
                mod.Log("GameSelectionPanel.competitorsTable: " + d.Get<AgeTransform>("competitorsTable"));
                mod.Log("GameSelectionPanel.competitorSlots: " + d.Get<List<CompetitorSlot>>("competitorSlots"));

                d.Set<int>("maxHeroCount", maxHeroShipCountWrapper.Value);

                mod.Log("After set:");
                mod.Log("GameSelectionPanel.maxHeroCount: " + d.Get<int>("maxHeroCount"));


                run = true;
            }
            orig(self);
        }

        private void MoviePanel_OnMoviePlaybackComplete(On.MoviePanel.orig_OnMoviePlaybackComplete orig, MoviePanel self)
        {
            // Should happen often enough (before we get to the PodScreen) in order to work out.
            if (!runInMovie)
            {
                // Set GameConfig
                SetGameConfig();

                runInMovie = true;
            }
            orig(self);
        }

        private void GameSelectionPanel_Display(On.GameSelectionPanel.orig_Display orig, GameSelectionPanel self, bool isMultiplayer, string mpSaveKey, int slotCount, GameSelectionPanel.GameSelectionFinishedHandler onGameSelectionFinished)
        {
            // Should happen often enough (before we get to the PodScreen) in order to work out.
            if (!run)
            {
                // Set GameConfig
                SetGameConfig();

                // Need to AT LEAST alter the following:
                // GameSelectionPanel.Display --> NullReference
                // Lift.Show() --> IndexOutOfBounds

                // GameSelectionPanel.Display --> NullReference
                var d = new DynData<GameSelectionPanel>(self);
                mod.Log("Before set:");
                mod.Log("GameSelectionPanel.maxHeroCount: " + d.Get<int>("maxHeroCount"));
                mod.Log("GameSelectionPanel.competitorsTable: " + d.Get<AgeTransform>("competitorsTable"));
                mod.Log("GameSelectionPanel.competitorSlots: " + d.Get<List<CompetitorSlot>>("competitorSlots"));

                d.Set<int>("maxHeroCount", maxHeroShipCountWrapper.Value);

                mod.Log("After set:");
                mod.Log("GameSelectionPanel.maxHeroCount: " + d.Get<int>("maxHeroCount"));

                run = true;
            }
            orig(self, isMultiplayer, mpSaveKey, slotCount, onGameSelectionFinished);
        }

        public void UnLoad()
        {
            mod.UnLoad();
            On.MoviePanel.OnMoviePlaybackComplete -= MoviePanel_OnMoviePlaybackComplete;
            On.GameSelectionPanel.Display -= GameSelectionPanel_Display;
            On.GameSelectionPanel.RefreshContent -= GameSelectionPanel_RefreshContent;
            // Remove hooks here!
        }
        private void SetLiftHeroes(Lift self)
        {
            LiftHero[] heroes = new LiftHero[Hero.GetLevelWinningHeroes().Count];
            for (int i = 0; i < heroes.Length; i++)
            {
                heroes[i] = self.LiftHeroes[i % self.LiftHeroes.Length];
                typeof(LiftHero).GetProperty("HeroName").SetValue(heroes[i], Hero.GetLevelWinningHeroes()[i].LocalizedName, null);
                mod.Log("Set Lift Hero: " + i + " to name: " + Hero.GetLevelWinningHeroes()[i].LocalizedName);
            }
            new DynData<Lift>(self).Set("liftHeroes", heroes);
            mod.Log("Set lift heroes!");
        }
        private void SetGameConfig()
        {
            // Need to AT LEAST set:
            // PlayerMaxHeroCount
            // MaxHeroCount
            // PlayerInitHeroCount
            GameConfig c = GameConfig.GetGameConfig();

            mod.Log("Before reflection:");
            mod.Log("MaxHeroCount: " + c.MaxHeroCount);
            mod.Log("PlayerMaxHeroCount: " + c.PlayerMaxHeroCount.Curve.Max);
            mod.Log("PlayerInitHeroCount: " + c.PlayerInitHeroCount.CurveOperation.Max);
            mod.Log("MultiplayerMaxPlayerCount: " + c.MultiplayerMaxPlayerCount);

            typeof(GameConfig).GetProperty("MaxHeroCount").SetValue(c, maxHeroCountWrapper.Value, null);
            typeof(GameConfig).GetProperty("PlayerMaxHeroCount").SetValue(
                c, CreateCurveDefinedValue(c.PlayerMaxHeroCount, maxHeroCountWrapper.Value), null);
            typeof(GameConfig).GetProperty("PlayerInitHeroCount").SetValue(
                c, CreateCurveDefinedValue(c.PlayerInitHeroCount, maxHeroShipCountWrapper.Value), null);
            typeof(GameConfig).GetProperty("MultiplayerMaxPlayerCount").SetValue(c, maxHeroShipCountWrapper.Value, null);

            mod.Log("After reflection:");
            mod.Log("MaxHeroCount: " + c.MaxHeroCount);
            mod.Log("PlayerMaxHeroCount: " + c.PlayerMaxHeroCount.Curve.Max);
            mod.Log("PlayerInitHeroCount: " + c.PlayerInitHeroCount.CurveOperation.Max);
            mod.Log("MultiplayerMaxPlayerCount: " + c.MultiplayerMaxPlayerCount);
        }
        private CurveOperation CreateCurveOperation(CurveOperation op, float value)
        {
            typeof(CurveOperation).GetProperty("Max").SetValue(op, value, null);
            typeof(CurveOperation).GetProperty("Min").SetValue(op, value, null);
            return op;
        }
        private Curve CreateCurve(Curve c, float value)
        {
            typeof(Curve).GetProperty("Max").SetValue(c, value, null);
            typeof(Curve).GetProperty("Min").SetValue(c, value, null);
            return c;
        }
        private CurveDefinedValue CreateCurveDefinedValue(CurveDefinedValue c, float value)
        {
            if (c.CurveOperation != null)
            {
                typeof(CurveDefinedValue).GetProperty("CurveOperation").SetValue(c, CreateCurveOperation(c.CurveOperation, value), null);
            }
            else if (c.Curve != null)
            {
                typeof(CurveDefinedValue).GetProperty("Curve").SetValue(c, CreateCurve(c.Curve, value), null);
            }
            return c;
        }
    }
}
