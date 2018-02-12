using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class SBFile
    {
        public string MyPath;
        public List<Tools.Entry> lines;
        public List<Bundle> bundles;

        public SBFile(string path)
        {
            MyPath = path;
            ReadFile();
            ProcessFile();
        }

        public void Save()
        {
            Save(MyPath);
        }

        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            foreach (Tools.Entry e in lines)
                Tools.WriteEntry(fs, e);
            fs.Close();
        }

        

        private void ReadFile()
        {
            FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read);
            lines = new List<Tools.Entry>();
            Tools.ReadEntries(fs, lines);
            fs.Close();
        }

        public void ProcessFile()
        {
            bundles = new List<Bundle>();
            foreach (Tools.Entry e in lines)
                if (e.type == 0x82)
                    foreach (Tools.Field f in e.fields)
                        if (f.fieldname == "bundles")
                            foreach (Tools.Entry b in (List<Tools.Entry>)f.data)
                                bundles.Add(Bundle.Create(b));
        }
    }
}
