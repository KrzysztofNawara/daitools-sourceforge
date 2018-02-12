using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;

namespace DA_Tool.BitTool
{
    

    public partial class BitTool : Form
    {
        public BitTool()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = textBox1.Text;
            s = s.Replace(" ", "");
            while (s.Length < 8)
                s += "0";
            byte[] data = Tools.StringToByteArray(s);
            MemoryStream m = new MemoryStream(data);
            m.Seek(0, 0);
            Tools.BinaryReader7Bit r = new Tools.BinaryReader7Bit(m);
            uint value = (uint)r.Read7BitEncodedInt();
            textBox2.Text = value.ToString("X");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s = textBox3.Text;
            s = s.Replace(" ", "");
            while (s.Length < 8)
                s = "0" + s;
            MemoryStream data = new MemoryStream(Tools.StringToByteArray(s));
            MemoryStream m = new MemoryStream();
            Tools.BinaryWriter7Bit w = new Tools.BinaryWriter7Bit(m);
            w.Write7BitEncodedInt(Tools.ReadLEInt(data));
            while (m.Length < 4)
                m.WriteByte(0);
            m.Seek(0, 0);
            s = "";
            int b;
            while ((b = m.ReadByte()) != -1 && b > 0)
                s += b.ToString("X2");
            textBox4.Text = s;
        }

        private void btnGenDJBHash_Click(object sender, EventArgs e)
        {
            try
            {
                uint uintHash = Tools.HashDJB232(txtDJBHashInput.Text);
                byte[] byteHash = BitConverter.GetBytes(uintHash);
                txtDJBHashOutput.Text = BitConverter.ToString(byteHash);
            }
            catch
            {
                txtDJBHashOutput.Text = "(error hashing)";
            }
        }
    }
}
