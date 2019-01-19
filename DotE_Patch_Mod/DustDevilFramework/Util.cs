using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DustDevilFramework
{
    public class Util
    {
        // Adds changes after startLocation with offset
        public static void ApplyLocalizationChange(string startLocation, int offset, string[] linesToWrite)
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
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
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        public static void ApplyLocalizationChange(string startLocation, int offset, List<string> linesToWrite)
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
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
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        // Deletes changes between startingPoint and endingPoint inclusive
        public static void RemoveLocalizationChangeInclusive(string startingPoint, string endingPoint)
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
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
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        // Deletes changes between startingPoint and endingPoint exclusive of the end, inclusive of the start
        public static void RemoveLocalizationChangeExclusiveEnd(string startingPoint, string endingPoint)
        {
            string[] lines = System.IO.File.ReadAllLines(@"Public\Localization\english\ED_Localization_Locales.xml");
            List<string> linesLst = lines.ToList();
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
            System.IO.File.WriteAllLines(@"Public\Localization\english\ED_Localization_Locales.xml", linesLst.ToArray());
        }
        // Returns a legible name from the given fieldinfo.Name
        public static string GetName(ScadMod m, FieldInfo f)
        {
            string name = f.Name;
            if (f.Name.Contains("<"))
            {
                // This is an autoproperty
                // This is okay, just make sure the name is reasonable!
                name = f.Name.Substring(f.Name.IndexOf("<") + 1, f.Name.LastIndexOf(">") - 1);
            }
            return m.name + " - " + name;
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
            foreach (Transform t in toDelete.transform)
            {
                DeleteChildrenInclusive(t.gameObject);
            }
        }
    }
}