using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class Bundle
    {
        public string path;
        public string salt;
        public List<ebxtype> ebx;
        public List<dbxtype> dbx;
        public List<restype> res;
        public List<chunktype> chunk;
        public bool align;
        public bool ridsupport;
        public bool compressed;
        public ulong totalsize;
        public ulong dbxtotalsize;

        public struct ebxtype
        {
            public string name;
            public byte[] SHA1;
            public byte[] size;
            public byte[] osize;
            public Tools.Entry link;
        }
        public struct dbxtype
        {
            public string name;
            public byte[] SHA1;
            public byte[] size;
            public byte[] osize;
            public Tools.Entry link;
        }
        public struct restype
        {
            public string name;
            public byte[] SHA1;
            public byte[] size;
            public byte[] osize;
            public byte[] rtype;
            public Tools.Entry link;
        }
        public struct chunktype
        {
            public byte[] id;
            public byte[] SHA1;
            public byte[] size;
            public Tools.Entry link;
        }

        public static Bundle Create(Tools.Entry e)
        {
            Bundle res = new Bundle();
            res.chunk = new List<chunktype>();
            foreach(Tools.Field f in e.fields)
                switch (f.fieldname)
                {
                    case "path":
                        res.path = (string)f.data;
                        break;
                    case "magicSalt":
                        res.salt = BitConverter.ToUInt32((byte[])f.data, 0).ToString("X4");
                        break;
                    case "alignMembers":
                        res.align = (bool)f.data;
                        break;
                    case "storeCompressedSizes":
                        res.compressed = (bool)f.data;
                        break;
                    case "totalSize":
                        res.totalsize = BitConverter.ToUInt64((byte[])f.data, 0);
                        break;
                    case "dbxtotalSize":
                        res.dbxtotalsize = BitConverter.ToUInt64((byte[])f.data, 0);
                        break;
                    case "ebx":
                        res.ebx = ReadEbx(f);
                        break;
                    case "dbx":
                        res.dbx = ReadDbx(f);
                        break;
                    case "res":
                        res.res = ReadRes(f);
                        break;
                    case "chunks":
                    case "chunks0":
                        res.chunk.AddRange(ReadChunks(f));
                        break;
                }
            return res;
        }
        private static List<ebxtype> ReadEbx(Tools.Field f)
        {
            List<ebxtype> res = new List<ebxtype>();
            List<Tools.Entry> list = (List<Tools.Entry>)f.data;
            foreach (Tools.Entry e in list)
            {
                ebxtype ebx = new ebxtype();
                ebx.link = e;
                foreach (Tools.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            ebx.name= (string)f2.data;
                            break;
                        case "sha1":
                            ebx.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            ebx.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            ebx.osize = (byte[])f2.data;
                            break;
                    }
                res.Add(ebx);
            }
            return res;
        }

        private static List<dbxtype> ReadDbx(Tools.Field f)
        {
            List<dbxtype> res = new List<dbxtype>();
            List<Tools.Entry> list = (List<Tools.Entry>)f.data;
            foreach (Tools.Entry e in list)
            {
                dbxtype dbx = new dbxtype();
                dbx.link = e;
                foreach (Tools.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            dbx.name = (string)f2.data;
                            break;
                        case "sha1":
                            dbx.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            dbx.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            dbx.osize = (byte[])f2.data;
                            break;
                    }
                res.Add(dbx);
            }
            return res;
        }

        private static List<restype> ReadRes(Tools.Field f)
        {
            List<restype> res = new List<restype>();
            List<Tools.Entry> list = (List<Tools.Entry>)f.data;
            foreach (Tools.Entry e in list)
            {
                restype r = new restype();
                r.link = e;
                foreach (Tools.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            r.name = (string)f2.data;
                            break;
                        case "sha1":
                            r.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            r.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            r.osize = (byte[])f2.data;
                            break;
                        case "resType":
                            r.rtype = (byte[])f2.data;
                            break;
                    }
                res.Add(r);
            }
            return res;
        }

        private static List<chunktype> ReadChunks(Tools.Field f)
        {
            List<chunktype> res = new List<chunktype>();
            List<Tools.Entry> list = (List<Tools.Entry>)f.data;
            foreach (Tools.Entry e in list)
            {
                chunktype c = new chunktype();
                c.link = e;
                foreach (Tools.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "id":
                            c.id = (byte[])f2.data;
                            break;
                        case "sha1":
                            c.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            c.size = (byte[])f2.data;
                            break;
                    }
                res.Add(c);
            }
            return res;
        }
    }


}
