using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools
{
    public static class Database
    {
        public static string dbpath = Path.GetDirectoryName(Application.ExecutablePath) + "\\database.sqlite";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection("Data Source=" + dbpath + ";Version=3;");
        }

        public static void CheckIfScanIsNeeded()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
            }
        }

        public static void CheckIfDBExists()
        {            
            if (!File.Exists(dbpath))
            {
                CreateDataBase();
            }
        }

        public static void CreateDataBase()
        {
            if (!File.Exists(dbpath))
                File.Delete(dbpath);
            SQLiteConnection.CreateFile(dbpath);
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLCommand("CREATE TABLE settings (key TEXT, value TEXT)", con);
            SQLCommand("INSERT INTO settings (key, value) values ('isNew', '1')", con);
            ClearSHA1db(con);
            ClearChunkdb(con);
            ClearSBFilesdb(con);
            ClearLangSBFilesdb(con);
            ClearTOCFilesdb(con);
            ClearLangTOCFilesdb(con);
            ClearCASFilesdb(con);
            ClearBundlesdb(con);
            con.Close();
        }        

        public static void LoadSettings()
        {
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAll("settings", con);
            GlobalStuff.settings = new Dictionary<string, string>();
            while (reader.Read())
                GlobalStuff.settings.Add(reader.GetString(0), reader.GetString(1));
            con.Close();
        }

        public static SQLiteDataReader getReader(string sql, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAll(string table, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllSorted(string table, string order, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + " ORDER BY " + order, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllWhere(string table, string where, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + " WHERE " + where, con);
            return command.ExecuteReader();
        }

        public static void SaveSettings()
        {
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLCommand("DROP TABLE settings", con);
            SQLCommand("CREATE TABLE settings (key TEXT, value TEXT)", con);
            foreach (KeyValuePair<string, string> setting in GlobalStuff.settings)
                SQLCommand("INSERT INTO settings (key, value) values ('" + setting.Key + "', '" + setting.Value + "')", con);
            con.Close();
            LoadSettings();
        }

        public static void ClearSHA1db(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS sha1db", con);
            SQLCommand("CREATE TABLE sha1db (line TEXT)", con);
        }

        public static void ClearSBFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS sbfiles", con);
            SQLCommand("CREATE TABLE sbfiles (path TEXT)", con);
        }

        public static void ClearLangSBFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS langsbfiles", con);
            SQLCommand("CREATE TABLE langsbfiles (path TEXT)", con);
        }

        public static void ClearLangTOCFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS langtocfiles", con);
            SQLCommand("CREATE TABLE langtocfiles (path TEXT)", con);
        }

        public static void ClearTOCFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS tocfiles", con);
            SQLCommand("CREATE TABLE tocfiles (path TEXT)", con);
        }

        public static void ClearCASFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS casfiles", con);
            SQLCommand("CREATE TABLE casfiles (path TEXT)", con);
        }

        public static void ClearChunkdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS chunkids", con);
            SQLCommand("CREATE TABLE chunkids (id TEXT, sha1 TEXT)", con);
        }

        public static void ClearBundlesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS bundles", con);
            SQLCommand("DROP TABLE IF EXISTS ebx", con);
            SQLCommand("DROP TABLE IF EXISTS res", con);
            SQLCommand("DROP TABLE IF EXISTS chunk", con);
            SQLCommand("CREATE TABLE bundles (id INTEGER PRIMARY KEY AUTOINCREMENT, filepath TEXT, frostpath TEXT, ebxcount INT, rescount INT, chunkcount INT)", con);
            SQLCommand("CREATE TABLE ebx (name TEXT, sha1 TEXT, bundle INT, type TEXT, guid TEXT, FOREIGN KEY (bundle) REFERENCES bundles (id))", con);
            SQLCommand("CREATE TABLE res (name TEXT, sha1 TEXT, rtype TEXT, bundle INT, FOREIGN KEY (bundle) REFERENCES bundles (id))", con);
            SQLCommand("CREATE TABLE chunk (id TEXT, sha1 TEXT, bundle INT, FOREIGN KEY (bundle) REFERENCES bundles (id))", con);
        }

        public static void AddSHA1(uint[] entry, SQLiteConnection con)
        {
            StringBuilder sb = new StringBuilder();
            foreach (uint u in entry)
                sb.Append(u.ToString("X8"));
            SQLCommand("INSERT INTO sha1db VALUES ('" + sb.ToString() + "')", con);
        }

        public static void AddChunk(byte[]id, byte[] sha1, SQLiteConnection con)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in id)
                sb.Append(b.ToString("X2"));
            StringBuilder sb2 = new StringBuilder();
            foreach (byte b in sha1)
                sb2.Append(b.ToString("X2"));
            SQLCommand("INSERT INTO chunkids (id,sha1) values ('" + sb.ToString() + "','" + sb2.ToString() + "')", con);
        }

        private static string[] exceptions = { "win32/da3/configurations/online/webbrowser/webbrowserbundle",
                                               "win32/da3/designcontent/prefabs/general/conversations/partybanter_listenerbundle"};

        public static void AddBundle(string filepath, Bundle b, SQLiteConnection con, CATFile cat)
        {
            
            if (b.ebx == null)
                b.ebx = new List<Bundle.ebxtype>();
            if (b.res == null)
                b.res = new List<Bundle.restype>();
            if (b.chunk == null)
                b.chunk = new List<Bundle.chunktype>();
            SQLCommand("INSERT INTO bundles VALUES (NULL,'" + filepath + "','" + b.path + "', " + b.ebx.Count + ", " + b.res.Count + ", " + b.chunk.Count + " )", con);
            SQLiteCommand command = new SQLiteCommand("SELECT last_insert_rowid()", con);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            long id = (long)reader.GetValue(0);
            var transaction = con.BeginTransaction();  
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            if(!exceptions.Contains(b.path))
                foreach (Bundle.ebxtype ebx in b.ebx)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (byte bb in ebx.SHA1)
                        sb.Append(bb.ToString("X2"));
                    List<uint> line = cat.FindBySHA1(ebx.SHA1);
                    string type = "";
                    string guid = "";
                    if (line.Count == 9)
                    {
                        CASFile cas = new CASFile(basepath + "cas_" + line[7].ToString("d2") + ".cas");
                        CASFile.CASEntry entry = cas.ReadEntry(line.ToArray());
                        try
                        {
                            /* Just obtain the Guid and Type from raw EBX */
                            Tools.ExtractEbxGuidAndType(new MemoryStream(entry.data), out type, out guid);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    SQLCommand("INSERT INTO ebx VALUES ('" + ebx.name.Replace("'", "") + "','" + sb.ToString() + "', " + id + ", '" + type + "', '" + guid + "')", con);
                }
            transaction.Commit();
            transaction = con.BeginTransaction();
            foreach (Bundle.restype res in b.res)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte bb in res.SHA1)
                    sb.Append(bb.ToString("X2"));
                uint restype = BitConverter.ToUInt32(res.rtype, 0);
                SQLCommand("INSERT INTO res VALUES ('" + res.name.Replace("'", "") + "','" + sb.ToString() + "', '" + restype.ToString("X8") + "', " + id + ")", con);
            }
            transaction.Commit();
            transaction = con.BeginTransaction();
            foreach (Bundle.chunktype chunk in b.chunk)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte bb in chunk.id)
                    sb.Append(bb.ToString("X2"));
                StringBuilder sb2 = new StringBuilder();
                foreach (byte bb2 in chunk.SHA1)
                    sb2.Append(bb2.ToString("X2"));
                SQLCommand("INSERT INTO chunk VALUES ('" + sb.ToString() + "', '" + sb2.ToString() + "', " + id + ")", con);
            }
            transaction.Commit();
        }

        public static void AddSBFile(string path, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO sbfiles VALUES ('" + path + "')", con);
        }

        public static void AddLanguageSBFile(string path, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO langsbfiles VALUES ('" + path + "')", con);
        }

        public static void AddTOCFile(string path, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO tocfiles VALUES ('" + path + "')", con);
        }

        public static void AddLanguageTOCFile(string path, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO langtocfiles VALUES ('" + path + "')", con);
        }

        public static void AddCASFile(string path, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO casfiles VALUES ('" + path + "')", con);
        }

        public static void SQLCommand(string sql, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand(sql, con);
            command.ExecuteNonQuery();
        }

        public static long SQLGetRowCount(string table, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) FROM " + table, con);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            return (long)reader.GetValue(0);            
        }

        public static byte[] getDataBySHA1(string sha1, SQLiteConnection con)
        {
            SQLiteDataReader reader = Database.getAllWhere("sha1db", "line LIKE '" + sha1 + "%'", con);
            if (!reader.Read())
                return new byte[0];
            string lines = reader.GetString(0);
            byte[] buff = Tools.StringToByteArray(lines);
            uint[] line = new uint[9];
            MemoryStream m = new MemoryStream(buff);
            m.Seek(20, 0);
            uint offset = Tools.ReadLEUInt(m);
            uint size = Tools.ReadLEUInt(m);
            uint casn = Tools.ReadLEUInt(m);
            reader = Database.getAllWhere("casfiles", "path LIKE '%cas_" + casn.ToString("d2") + ".cas'", con);
            if (!reader.Read())
                return new byte[0];
            CASFile cas = new CASFile(reader.GetString(0));
            CASFile.CASEntry entry = cas.ReadEntry(offset, size);
            return entry.data;
        }
    }
}
