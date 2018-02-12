/*
 * dailuac DIA tool Lua script compiler.
 * http://daitools.freeforums.org/
 * 
 ** $Id: luac.c,v 1.54 2006/06/02 17:37:11 lhf Exp $
 ** Lua compiler (saves bytecodes to files; also list bytecodes)
 ** See Copyright Notice in lua.h
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using KopiLua;

namespace KopiLua
{
    using Instruction = System.UInt32;

    public class Program
    {                    
        public class Smain
        {
            public int argc;
            public string[] argv;          
        };

        static void fatal(Lua.CharPtr message)
        {
            Lua.fprintf(Lua.stderr, "dailuac: %s\n", message);
            Environment.Exit(Lua.EXIT_FAILURE);
        }

        static void cannot(Lua.CharPtr output, Lua.CharPtr what)
        {
            Lua.fprintf(Lua.stderr, "dailuac: cannot %s %s: %s\n", what, output, Lua.strerror(Lua.errno()));
            Environment.Exit(Lua.EXIT_FAILURE);
        }

        static int writer(Lua.lua_State L, Lua.CharPtr p, uint size, object u)
        {
            return ((Lua.fwrite(p, (int)size, 1, (Stream)u) != 1) && (size != 0)) ? 1 : 0;
        }

        static Lua.Proto toproto(Lua.lua_State L, int i) { return Lua.clvalue(L.top + (i)).l.p; }

        static int pmain(Lua.lua_State L)
        {
            Smain s = (Smain) Lua.lua_touserdata(L, 1);            
                    
            Lua.CharPtr inputPath = s.argv[0];
            string fileText = System.IO.File.ReadAllText(inputPath.ToString());
         
            // Extract function name   
            string funcName = "";
            Match m = Regex.Match(fileText, @"function (.+)\(");                        
            if (m.Success)            
                funcName = m.Result("$1");            
            else
                Lua.luaL_error(L, "input file missing function definition!");

            // Extract function argument(s) if any
            string argsStr = "";
            m = Regex.Match(fileText, @"function .+\((.+)\)");
            if (m.Success)           
                argsStr = Regex.Replace(m.Result("$1"), @"\s", "");            
          
            if (Lua.luaL_loadfile_DAI(L, inputPath, fileText) != 0) 
                fatal(Lua.lua_tostring(L, -1));

            Lua.CharPtr outputPath = Path.ChangeExtension(inputPath.ToString(), ".luac");
            Stream D = Lua.fopen(outputPath, "wb");
            if (D == null) 
                cannot(outputPath, "open");           

            // Write DAI header            
            BinaryWriter bw = new BinaryWriter(D);
            bw.Write((uint) 0xE1850009);
            bw.Write((uint) 1);
            bw.Write((uint) funcName.Length + 1);
            uint argsCnt = 0;            
            if (argsStr != "")
            {
                argsCnt = 1;
                foreach (char c in argsStr)
                    if (c == ',') argsCnt++;
            }            
            bw.Write((uint) argsStr.Length + 1);
            bw.Write((uint) argsCnt);
            long dataSizePos = bw.BaseStream.Position;
            bw.Write((uint) 0);
            bw.Write((char[]) funcName.ToCharArray(), 0, funcName.Length);
            bw.Write((byte) 0);
            if (argsCnt > 0)           
                bw.Write((char[]) argsStr.ToCharArray(), 0, argsStr.Length);                        
            bw.Write((byte)0);           
           
            // Write luac data
            Lua.Proto f = toproto(L, -1);
            long startPos = D.Position;     
            Lua.luaU_dump(L, f, writer, D, 0);            
            if (Lua.ferror(D) != 0) cannot(outputPath, "write");
            // Update data size in DAI header
            long dataSize = (D.Position - startPos);
            bw.Seek((int) dataSizePos, SeekOrigin.Begin);
            bw.Write((uint) dataSize);

            if (Lua.fclose(D) != 0) cannot(outputPath, "close");           
            return 0;
        }

        static int Main(string[] args)
        {                  
            int argc = args.Length;
            if (argc != 1)
            {
                Lua.fprintf(Lua.stderr, "<< DAI Lua script compile tool ver: %s >>\n", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                Lua.fprintf(Lua.stderr, "   https://sourceforge.net/projects/daitools/\n");
                Lua.fprintf(Lua.stderr, "  Usage: inputFile\n");
                Lua.fprintf(Lua.stderr, "Example: dailuac compute_random_script.lua\n");
                return Lua.EXIT_FAILURE;
            }            
            if(!File.Exists(args[0]))
            {
                Lua.fprintf(Lua.stderr, "Input Lua file not found!\n");
                return Lua.EXIT_FAILURE;
            }

            Lua.lua_State L = Lua.lua_open();
            if (L == null) 
                fatal("not enough memory for Lua state");

            Smain s = new Smain();
            s.argc = argc; s.argv = args;            
            if (Lua.lua_cpcall(L, pmain, s) != 0) 
                fatal(Lua.lua_tostring(L, -1));

            Lua.lua_close(L);
            return Lua.EXIT_SUCCESS;
        }
    }
}