using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Threading;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools
{
    public interface ModScript
    {
        void RunScript();
    }

    public static class Scripting
    {
        private static RichTextBox box;
        private static readonly object _sync = new object();
        private static List<byte[]> data = new List<byte[]>();
        private static Mod.ModMetaData meta;

        public static Assembly CompileCode(string code)
        {
            Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> dlls = new List<string>(Directory.GetFiles(path, "*.dll"));
            for(int i=0;i<dlls.Count;i++)
                switch (Path.GetFileName(dlls[i]))
                {
                    case "DevIL.dll":
                        dlls.RemoveAt(i);
                        break;
                }
            options.ReferencedAssemblies.AddRange(dlls.ToArray());
            options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            options.ReferencedAssemblies.Add("System.Data.Linq.dll");
            CompilerResults result = null;
            try
            {
                result = csProvider.CompileAssemblyFromSource(options, code);
            }
            catch (Exception exc)
            {
                throw new Exception("Exception caught: " + exc.Message);
            }
            if (result.Errors.HasErrors)
            {
                string error = "";
                error += "Line: " + result.Errors[0].Line + "  Column: " + result.Errors[0].Column + "\n";
                error += "(" + result.Errors[0].ErrorNumber + ")\n" + result.Errors[0].ErrorText;
                throw new Exception(error);
            }
            if (result.Errors.HasWarnings)
            {
            }
            return result.CompiledAssembly;
        }

        public static Thread RunScriptThreaded(string code)
        {
            Thread t = new Thread(threadScript);
            t.Start(code);
            return t;
        }

        private static void threadScript(object objs)
        {
            RunScript((string)objs);
        }

        public static void RunScript(string code)
        {
            LogLn("Compiling...");
            Assembly ass = null;
            try
            {             
                ass = CompileCode(code);            
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return;
            }
            Clear(true);
            foreach (Type type in ass.GetExportedTypes())
                foreach (Type iface in type.GetInterfaces())
                    if (iface == typeof(ModScript))
                    {
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if (constructor != null && constructor.IsPublic)
                        {
                            ModScript scriptObject = constructor.Invoke(null) as ModScript;
                            if (scriptObject != null)
                                try
                                {
                                    scriptObject.RunScript();
                                    return;
                                }
                                catch(Exception ex)
                                {
                                    Log(ex.ToString());
                                }
                        }
                    }
            Log("Error: Script could not be executed");
        }

        public static void SetScriptOutput(RichTextBox rtb)
        {
            box = rtb;
        }

        public static void SetData(List<byte[]> d)
        {
            lock (_sync)
            {
                data = d;
            }
        }

        public static List<byte[]> GetData()
        {
            lock (_sync)
            {
                return data;
            }
        }

        public static byte[] GetDataEntry(int n)
        {
            lock (_sync)
            {
                if (n >= 0 && n < data.Count)
                    return data[n];
                else
                    return new byte[0];
            }
        }

        public static void SetMeta(Mod.ModMetaData m)
        {
            lock (_sync)
            {
                meta = m;
            }
        }

        public static Mod.ModMetaData GetMeta()
        {
            lock (_sync)
            {
                return meta;
            }
        }

        public static void Log(string s, bool update = false)
        {
            lock (_sync)
            {
                if (box != null)
                    box.BeginInvoke(new Action(delegate
                    {
                        box.AppendText(s);
                        box.SelectionStart = box.Text.Length;
                        box.ScrollToCaret();
                        if (update)
                        {
                            box.Refresh();
                            Application.DoEvents();
                        }
                    }));
            }
        }

        public static void LogLn(string s, bool update = false)
        {
            Log(s + "\n", update);
        }

        public static void Clear(bool update = false)
        {
            lock (_sync)
            {
                if (box != null)
                    box.BeginInvoke(new Action(delegate
                    {
                        box.Text = "";
                        box.SelectionStart = 0;
                        box.ScrollToCaret();
                        if (update)
                        {
                            box.Refresh();
                            Application.DoEvents();
                        }
                    }));
            }
        }
    }
}
