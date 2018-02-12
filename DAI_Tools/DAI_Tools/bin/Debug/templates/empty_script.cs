using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace DAI_Tools
{
    public class MyMod : ModScript
    {
        public void RunScript()
        {
            Scripting.LogLn("Code works", true);
            for (int i = 0; i < 30; i++)
            {
                Scripting.Log(".", true);
                Thread.Sleep(100);
            }            
        }
    }
}