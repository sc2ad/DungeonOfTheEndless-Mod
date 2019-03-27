using DustDevilFramework;
using UnityEngine;

namespace TASTools_Mod
{
    public class TASToolsSettings : ModSettings
    {
        public string TasFileExtension = @".tas";
        public string PlayKey = KeyCode.Quote.ToString();
        public string RecordKey = KeyCode.Semicolon.ToString();
        public string PauseKey = KeyCode.P.ToString();
        public string ResetKey = KeyCode.RightBracket.ToString();
        public string SaveKey = KeyCode.LeftBracket.ToString();
        public string SaveToFilesKey = KeyCode.O.ToString();
        public string ReadFromFilesKey = KeyCode.Slash.ToString();
        public string ClearKey = KeyCode.C.ToString();

        public TASToolsSettings(string name) : base(name)
        {
        }
    }
}
