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
using System.Xml.Linq;
using System.Xml;

namespace DA_Tool.DASReader
{
    public partial class DASReader : Form
    {
        public DASReader()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.das|*.das";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Read(d.FileName);
            }
        }

        public void Read(string path)
        {
            rtb1.Text = string.Empty;
            rtbFaceXML.Text = string.Empty;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                DisplayMainDASScreen(fs);
                DisplayFaceXML(fs);
            }
        }

        private void DisplayMainDASScreen(FileStream fs)
        {
            fs.Seek(0x20, 0);
            int count = Tools.ReadLEInt(fs);
            for (int i = 0; i < count; i++)
            {
                string s = "0x" + Tools.ReadLEUInt(fs).ToString("X8") + " ";
                ushort len = Tools.ReadLEUShort(fs);
                for (int j = 0; j < len; j++)
                    s += (char)fs.ReadByte();
                s += "\n";
                rtb1.AppendText(s);
            }
        }

        private void DisplayFaceXML(FileStream fs)
        {
            fs.Seek(-1, SeekOrigin.End);
            int shift = GetShiftAmount((byte)fs.ReadByte());

            byte[] faceXMLBlock = new byte[25000];
            fs.Seek(-25000, SeekOrigin.End);
            if (fs.Read(faceXMLBlock, 0, 25000) != 25000)
            {
                rtbFaceXML.Text = "Error";
            }

            string XML = ShiftBytes(shift, faceXMLBlock);

            try
            {
                rtbFaceXML.Text = MakeXmlReadable(XML);
            }
            catch
            {
                rtbFaceXML.Text = "Error";
            }
        }

        private string ShiftBytes(int shift, byte[] faceXMLBlock)
        {
            Array.Reverse(faceXMLBlock);
            List<byte> ShiftedBytes = new List<byte>();
            byte currentByte = 0xFF;
            for (int byteCount = 0; byteCount < faceXMLBlock.Length - 1; byteCount++)
            {
                currentByte = (byte)(faceXMLBlock[byteCount + 1] << (8 - shift));
                byte nextByte = (byte)(faceXMLBlock[byteCount] >> shift);
                currentByte = (byte)(nextByte | currentByte);
                if (currentByte == 0)
                {
                    break;
                }
                ShiftedBytes.Add(currentByte);
            }
            byte[] OriginalOrderShiftedBytes = ShiftedBytes.ToArray();
            Array.Reverse(OriginalOrderShiftedBytes);
            string XMLPlusExtraBytes = System.Text.Encoding.ASCII.GetString(OriginalOrderShiftedBytes);
            return XMLPlusExtraBytes.Substring(XMLPlusExtraBytes.IndexOf('<'));
        }

        private string MakeXmlReadable(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        private int GetShiftAmount(byte testByte)
        {
            int shift = 1;
            while (shift < 8)
            {
                byte testForBit = (byte)(1 << shift);
                if ((testForBit & testByte) > 0)
                {
                    break;
                }
                shift++;
            }
            return shift - 1;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
