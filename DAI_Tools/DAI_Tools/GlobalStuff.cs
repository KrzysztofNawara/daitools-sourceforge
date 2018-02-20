using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAI_Tools.Frostbite;

namespace DAI_Tools
{
    public static class GlobalStuff
    {
        public static Dictionary<string, string> settings;
        private static CATFile cat;

        public static string FindSetting(string name)
        {
            foreach (KeyValuePair<string, string> setting in settings)
                if (setting.Key == name)
                    return setting.Value;
            return "";
        }

        public static void AssignSetting(string key, string value)
        {
            settings[key] = value;
            Database.SaveSettings();
        }

        public static CATFile getCatFile()
        {
            if (cat == null)
            {
                string path = GlobalStuff.FindSetting("gamepath");
                path += "Data\\cas.cat";
                cat = new CATFile(path);
            }
            
            return cat;
        }
    }
}
