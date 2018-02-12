using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Windows.Forms;

namespace DAI_Tools.Frostbite
{
    public class Mod
    {
        public class Modjob
        {
            public string name;
            public string xml;
            public string script;
            public List<byte[]> data;
            public ModMetaData meta;
            public Modjob()
            {
            }
            public Modjob(string _name, string _xml, string _script, List<byte[]> _data, ModMetaData _meta)
            {
                name = _name;
                xml = _xml;
                script = _script;
                data = _data;
                meta = _meta;
            }
        }
        public class ModMetaData
        {
            public byte version;
            public string id;
            public ModDetail details;
            public List<ModReq> requirements;
            public List<ModBundle> bundles;

            public ModMetaData(byte _version, string _id, ModDetail _details, List<ModReq> _reqirements, List<ModBundle> _bundles)
            {
                version = _version;
                id = _id;
                details = _details;
                requirements = _reqirements;
                bundles = _bundles;
            }
        }
        public class ModDetail
        {
            public string name;
            public byte version;
            public string author;
            public string description;
            public ModDetail(string _name, byte _version, string _author, string _description)
            {
                name = _name;
                version = _version;
                author = _author;
                description = _description;
            }
        }
        public class ModReq
        {
            public string id;
            public string minVersion;
            public ModReq(string _id, string _minVersion)
            {
                id = _id;
                minVersion = _minVersion;
            }
        }
        public class ModBundle
        {
            public string name;
            public string action;
            public List<ModBundleEntry> entries;
            public ModBundle(string _name, string _action, List<ModBundleEntry> _entries)
            {
                name = _name;
                action = _action;
                entries = _entries;
            }
        }
        public class ModBundleEntry
        {
            public string name;
            public string action;
            public string orgSHA1;
            public byte resId;
            public ModBundleEntry(string _name, string _action, string _orgSHA1, byte _resId)
            {
                name = _name;
                action = _action;
                orgSHA1 = _orgSHA1;
                resId = _resId;
            }
        }
        public static string basepath = Application.StartupPath + "\\";

        public List<Modjob> jobs;

        public void Save(string path)
        {
            SaveMods(path, jobs);
        }
        public void SaveMods(string path, List<Modjob> MODs)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Tools.WriteInt(fs, 0x4D494144);
            Tools.WriteInt(fs, 0x3256444F);
            Tools.WriteInt(fs, MODs.Count);
            foreach (Modjob mod in MODs)
            {
                Tools.WriteNullString(fs, mod.name);
                Tools.WriteNullString(fs, mod.xml);
                Tools.WriteNullString(fs, mod.script);
                if (mod.data != null)
                {
                    Tools.WriteInt(fs, mod.data.Count);
                    foreach (byte[] data in mod.data)
                    {
                        Tools.WriteInt(fs, data.Length);
                        fs.Write(data, 0, data.Length);
                    }
                }
                else
                    Tools.WriteInt(fs, 0);
            }
            fs.Close();
        }
        public void Load(string path)
        {
            jobs = new List<Modjob>();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            int magic1 = Tools.ReadInt(fs);
            int magic2 = Tools.ReadInt(fs);
            if (magic1 != 0x4D494144 || magic2 != 0x3256444F)
                throw new Exception ("Not a valid mod file!");
            int count = Tools.ReadInt(fs);
            for (int i = 0; i < count; i++)
            {
                Modjob mod = new Modjob();
                mod.name = Tools.ReadNullString(fs);
                mod.xml = Tools.ReadNullString(fs);
                mod.script = Tools.ReadNullString(fs);
                int datacount = Tools.ReadInt(fs);
                mod.data = new List<byte[]>();
                for (int j = 0; j < datacount; j++)
                {
                    int len = Tools.ReadInt(fs);
                    byte[] data = new byte[len];
                    fs.Read(data, 0, len);
                    mod.data.Add(data);
                }
                jobs.Add(mod);
            }
            fs.Close();
        }
        public static string MakeXMLfromJobMeta(ModMetaData meta)
        {
            XMLHelper.Node root = new XMLHelper.Node();
            root.name = "daimod";
            root.properties = new List<XMLHelper.NodeProp>();
            root.properties.Add(new XMLHelper.NodeProp("version", meta.version.ToString()));
            root.properties.Add(new XMLHelper.NodeProp("id", meta.id));    
            root.childs = new List<XMLHelper.Node>();
            XMLHelper.Node details = new XMLHelper.Node();
            details.name = "details";
            details.childs = new List<XMLHelper.Node>();
            details.childs.Add(new XMLHelper.Node("name", meta.details.name));
            details.childs.Add(new XMLHelper.Node("version", meta.details.version.ToString()));
            details.childs.Add(new XMLHelper.Node("author", meta.details.author));
            details.childs.Add(new XMLHelper.Node("description", meta.details.description));
            root.childs.Add(details);
            XMLHelper.Node req = new XMLHelper.Node();
            req.name = "requirements";
            req.childs = new List<XMLHelper.Node>();
            foreach (ModReq r in meta.requirements)
            {
                XMLHelper.Node rn = new XMLHelper.Node("requires", "");
                rn.properties = new List<XMLHelper.NodeProp>();
                rn.properties.Add(new XMLHelper.NodeProp("id", r.id));
                if (r.minVersion != "")
                    rn.properties.Add(new XMLHelper.NodeProp("minVersion", r.minVersion));
                req.childs.Add(rn);
            }
            root.childs.Add(req);
            XMLHelper.Node bundles = new XMLHelper.Node();
            bundles.name = "bundles";
            bundles.childs = new List<XMLHelper.Node>();
            foreach (ModBundle b in meta.bundles)
            {
                XMLHelper.Node bn = new XMLHelper.Node("bundle", "");
                bn.properties = new List<XMLHelper.NodeProp>();
                bn.properties.Add(new XMLHelper.NodeProp("name", b.name));
                bn.properties.Add(new XMLHelper.NodeProp("action", b.action));
                bn.childs = new List<XMLHelper.Node>();
                XMLHelper.Node eln = new XMLHelper.Node("entries", "");
                eln.childs = new List<XMLHelper.Node>();
                foreach (ModBundleEntry e in b.entries)
                {
                    XMLHelper.Node en = new XMLHelper.Node("entry", "");
                    en.properties = new List<XMLHelper.NodeProp>();
                    en.properties.Add(new XMLHelper.NodeProp("name", e.name));
                    en.properties.Add(new XMLHelper.NodeProp("action", e.action));
                    en.properties.Add(new XMLHelper.NodeProp("originalSha1", e.orgSHA1));
                    en.properties.Add(new XMLHelper.NodeProp("resourceId", e.resId.ToString()));
                    eln.childs.Add(en);
                }
                bn.childs.Add(eln);
                bundles.childs.Add(bn);
            }
            root.childs.Add(bundles);
            return XMLHelper.MakeXML(root, 0);
        }

        public static ModMetaData MakeMetafromJobXML(string xml)
        {
            if (!XMLHelper.Validate(xml, File.ReadAllText(basepath + "templates\\validate.xsd")))
                return null;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            ModDetail detail = null;
            List<ModReq> requirements = new List<ModReq>();
            List<ModBundle> bundles = new List<ModBundle>();
            foreach (XmlNode n in dom.ChildNodes[0].ChildNodes)
            {
                switch (n.Name)
                {
                    case "details":
                        detail = new ModDetail("", 0, "", "");
                        foreach (XmlNode child in n.ChildNodes)
                            switch (child.Name)
                            {
                                case "name":
                                    detail.name = child.InnerText;
                                    break;
                                case "version":
                                    detail.version = Convert.ToByte(child.InnerText);
                                    break;
                                case "author":
                                    detail.author = child.InnerText;
                                    break;
                                case "description":
                                    detail.description = child.InnerText;
                                    break;
                            }
                        break;
                    case "requirements":
                        foreach (XmlNode child in n.ChildNodes)
                            if (child.Name == "requires")
                            {
                                ModReq req = new ModReq("", "");
                                foreach (XmlAttribute a in child.Attributes)
                                    switch (a.Name)
                                    {
                                        case "id":
                                            req.id = a.Value;
                                            break;
                                        case "minVersion":
                                            req.minVersion = a.Value;
                                            break;
                                    }
                            }
                        break;
                    case "bundles":                        
                        foreach (XmlNode child in n.ChildNodes)
                            if(child.Name == "bundle")
                        {
                            ModBundle bundle = new ModBundle("", "", null);
                            foreach (XmlAttribute a in child.Attributes)
                                switch (a.Name)
                                {
                                    case "name":
                                        bundle.name = a.Value;
                                        break;
                                    case "action":
                                        bundle.action = a.Value;
                                        break;
                                }
                            bundle.entries = new List<ModBundleEntry>();
                            foreach (XmlNode child2 in child.ChildNodes[0])
                            {
                                ModBundleEntry entry = new ModBundleEntry("", "", "", 0);
                                foreach (XmlAttribute a in child2.Attributes)
                                    switch (a.Name)
                                    {
                                        case "name":
                                            entry.name = a.Value;
                                            break;
                                        case "action":
                                            entry.action = a.Value;
                                            break;
                                        case "originalSha1":
                                            entry.orgSHA1 = a.Value;
                                            break;
                                        case "resourceId":
                                            entry.resId = Convert.ToByte(a.Value);
                                            break;
                                    }
                                bundle.entries.Add(entry);
                            }
                            bundles.Add(bundle);
                        }
                        break;
                }
            }
            byte version = 1;
            string id = "";
            foreach (XmlAttribute a in dom.ChildNodes[0].Attributes)
                switch (a.Name)
                {
                    case "version":
                        version = Convert.ToByte(a.Value);
                        break;
                    case "id":
                        id = a.Value;
                        break;
                }
            ModMetaData result = new ModMetaData(version, id, detail, requirements, bundles);
            return result;
        }

        public static string GetOrSetAuthor()
        {
            string name = GlobalStuff.FindSetting("author");
            if (name == "")
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter author name", "Author name", "noname");
                if (input != "")
                {
                    name = input;
                    GlobalStuff.AssignSetting("author", name);
                }
                else
                    name = "noname";
            }
            return name;
        }
    }
}
