using BepInEx.Configuration;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DustDevilFramework
{
    public class Util
    {
        public static ConfigFile GetConfigFile(ScadMod mod)
        {
            try
            {
                return new DynData<BepInEx.BaseUnityPlugin>(mod.BepinPluginReference).Get<ConfigFile>("Config");
            }
            catch (Exception _)
            {
                mod.Log(BepInEx.Logging.LogLevel.Error, "Could not find config file for plugin with name: " + mod.name);
                string configPath = @"BepInEx\config\" + mod.name + ".cfg";
                mod.Log(BepInEx.Logging.LogLevel.Warning, "Attempting to use default config: " + configPath);
                return new ConfigFile(configPath, true);
            }
        }
        public static List<T> GetList<T>(T[] arr)
        {
            List<T> toReturn = new List<T>();
            foreach (T item in arr)
            {
                toReturn.Add(item);
            }
            return toReturn;
        }
        // Applies a change to a file
        public static void ApplyFileChange(string filename, string startLocation, int offset, string[] linesToWrite)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            List<string> linesLst = GetList(lines);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf(linesToWrite[0]) != -1)
                {
                    // The changes already exist
                    return;
                }
                if (line.IndexOf(startLocation) != -1)
                {
                    for (int q = 0; q < linesToWrite.Length; q++)
                    {
                        linesLst.Insert(i + offset + q, linesToWrite[q]);
                    }
                }
            }
            System.IO.File.WriteAllLines(filename, linesLst.ToArray());
        }
        public static void ApplyFileChange(string filename, string startLocation, int offset, List<string> linesToWrite)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            List<string> linesLst = GetList(lines);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf(linesToWrite[0]) != -1)
                {
                    // The changes already exist
                    return;
                }
                if (line.IndexOf(startLocation) != -1)
                {
                    for (int q = 0; q < linesToWrite.Count; q++)
                    {
                        linesLst.Insert(i + offset + q, linesToWrite[q]);
                    }
                }
            }
            System.IO.File.WriteAllLines(filename, linesLst.ToArray());
        }
        // Adds changes after startLocation with offset
        public static void ApplyLocalizationChange(string startLocation, int offset, string[] linesToWrite)
        {
            ApplyFileChange(@"Public\Localization\english\ED_Localization_Locales.xml", startLocation, offset, linesToWrite);
        }
        public static void ApplyLocalizationChange(string startLocation, int offset, List<string> linesToWrite)
        {
            ApplyFileChange(@"Public\Localization\english\ED_Localization_Locales.xml", startLocation, offset, linesToWrite);
        }
        // Deletes changes from a file
        public static void RemoveFileChangeInclusive(string filename, string startingPoint, string endingPoint)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            List<string> linesLst = GetList(lines);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf(startingPoint) != -1)
                {
                    int end = i;
                    for (int j = i; j < lines.Length; j++)
                    {
                        if (lines[j].IndexOf(endingPoint) != -1)
                        {
                            end = j;
                            break;
                        }
                    }
                    for (int l = i; l <= end; l++)
                    {
                        linesLst.RemoveAt(i);
                    }
                    break;
                }
            }
            System.IO.File.WriteAllLines(filename, linesLst.ToArray());
        }
        public static void RemoveFileChangeExclusiveEnd(string filename, string startingPoint, string endingPoint)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            List<string> linesLst = GetList(lines);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.IndexOf(startingPoint) != -1)
                {
                    int end = i;
                    for (int j = i; j < lines.Length; j++)
                    {
                        if (lines[j].IndexOf(endingPoint) != -1)
                        {
                            end = j;
                            break;
                        }
                    }
                    for (int l = i; l < end; l++)
                    {
                        linesLst.RemoveAt(i);
                    }
                    break;
                }
            }
            System.IO.File.WriteAllLines(filename, linesLst.ToArray());
        }
        // Deletes changes between startingPoint and endingPoint inclusive
        public static void RemoveLocalizationChangeInclusive(string startingPoint, string endingPoint)
        {
            RemoveFileChangeInclusive(@"Public\Localization\english\ED_Localization_Locales.xml", startingPoint, endingPoint);
        }
        // Deletes changes between startingPoint and endingPoint exclusive of the end, inclusive of the start
        public static void RemoveLocalizationChangeExclusiveEnd(string startingPoint, string endingPoint)
        {
            RemoveFileChangeExclusiveEnd(@"Public\Localization\english\ED_Localization_Locales.xml", startingPoint, endingPoint);
        }
        // Returns a legible name from the given fieldinfo.Name
        internal static string GetName(ScadMod m, BepInExSettingsPacket packet)
        {
            return m.name + " - " + packet.Name;
        }
        // Deletes all the children of a certain GameObject including the provided GameObject
        public static void DeleteChildrenInclusive(GameObject toDelete)
        {
            // Delete children, then self
            foreach (Transform t in toDelete.transform)
            {
                DeleteChildrenInclusive(t.gameObject);
            }
            GameObject.DestroyImmediate(toDelete);
        }
        // Deletes all the children of a certain GameObject excluding the provided GameObject
        public static void DeleteChildrenExclusive(GameObject toDelete)
        {
            for (int i = toDelete.transform.childCount - 1; i >= 0; i--)
            {
                Transform t = toDelete.transform.GetChild(i);
                DeleteChildrenInclusive(t.gameObject);
            }
        }
        // Logs lots of information about a given AgeTransform (in order to resolve issues with alignment)
        public static void LogVariousAgeTransformInfo(AgeTransform trans)
        {
            Debug.Log("========================================== LOG FOR: " + trans.name + " ==========================================");
            Debug.Log("Anchored: " + trans.Anchored);
            Debug.Log("Attach Bottom: " + trans.AttachBottom);
            Debug.Log("Attach Left: " + trans.AttachLeft);
            Debug.Log("Attach Right: " + trans.AttachRight);
            Debug.Log("Attach Top: " + trans.AttachTop);
            Debug.Log("AutoResizeHeight: " + trans.AutoResizeHeight);
            Debug.Log("AutoResizeWidth: " + trans.AutoResizeWidth);
            Debug.Log("ChildHeight: " + trans.ChildHeight);
            Debug.Log("ChildWidth: " + trans.ChildWidth);
            Debug.Log("ChildComparer: " + trans.ChildrenComparer);
            Debug.Log("FixedSize: " + trans.FixedSize);
            trans.ComputeGlobalPosition(out Rect temp);
            Debug.Log("GlobalPosition: " + temp);
            Debug.Log("DirtyPosition: " + trans.DirtyPosition);
            Debug.Log("RenderedPosition: " + trans.GetRenderedPosition());
            Debug.Log("HasModifiers: " + trans.HasModifiers);
            Debug.Log("Height: " + trans.Height);
            Debug.Log("HorizontalMargin: " + trans.HorizontalMargin);
            Debug.Log("HorizontalSpacing: " + trans.HorizontalSpacing);
            Debug.Log("ModifiersRunning: " + trans.ModifiersRunning);
            Debug.Log("NoOverroll: " + trans.NoOverroll);
            Debug.Log("Percent Bottom: " + trans.PercentBottom);
            Debug.Log("Percent Left: " + trans.PercentLeft);
            Debug.Log("Percent Right: " + trans.PercentRight);
            Debug.Log("Percent Top: " + trans.PercentTop);
            Debug.Log("PivotMode: " + trans.PivotMode);
            Debug.Log("PivotOffset: " + trans.PivotOffset);
            Debug.Log("PixelMarginBottom: " + trans.PixelMarginBottom);
            Debug.Log("PixelMarginLeft: " + trans.PixelMarginLeft);
            Debug.Log("PixelMarginRight: " + trans.PixelMarginRight);
            Debug.Log("PixelMarginTop: " + trans.PixelMarginTop);
            Debug.Log("PixelOffsetBottom: " + trans.PixelOffsetBottom);
            Debug.Log("PixelOffsetLeft: " + trans.PixelOffsetLeft);
            Debug.Log("PixelOffsetRight: " + trans.PixelOffsetRight);
            Debug.Log("PixelOffsetTop: " + trans.PixelOffsetTop);
            Debug.Log("Position: " + trans.Position);
            Debug.Log("PropagateDirty: " + trans.PropagateDirty);
            Debug.Log("TableArrangement: " + trans.TableArrangement);
            Debug.Log("Tag: " + trans.tag);
            Debug.Log("TiltAngle: " + trans.TiltAngle);
            Debug.Log("UniformScale: " + trans.UniformScale);
            Debug.Log("VerticalMargin: " + trans.VerticalMargin);
            Debug.Log("VerticalSpacing: " + trans.VerticalSpacing);
            Debug.Log("Visible: " + trans.Visible);
            Debug.Log("Width: " + trans.Width);
            Debug.Log("X: " + trans.X);
            Debug.Log("Y: " + trans.Y);
            Debug.Log("Z: " + trans.Z);
            Debug.Log("Children Count: " + trans.GetChildren().Count);
            foreach (AgeTransform t in trans.GetChildren())
            {
                Debug.Log("- " + t.transform);
            }
            Debug.Log("====================================================================================");
        }
    }
}