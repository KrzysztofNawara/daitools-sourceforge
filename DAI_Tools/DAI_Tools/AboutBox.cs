using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAI_Tools
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            string text = "\n.\n..\n...\nDAI Tools Credits\n\n\n";
            text += "Ehamloptiran:\n";
            text += "============\n";
            text += "-Shader Explorer\n";
            text += "-Research\n\n\n";
            text += "Wogoodes:\n";
            text += "=========\n";
            text += "-Research\n\n\n";
            text += "Warranty Voider:\n";
            text += "================\n";
            text += "-Research\n";
            text += "-Database Manager\n";
            text += "-Bundle Browser\n";
            text += "-Sound Explorer\n";
            text += "-Talktable Explorer\n";
            text += "-Talktable Editor\n";
            text += "-Texture Explorer\n\n\n";
            text += "Sirmabus:\n";
            text += "=========\n";
            text += "-Script Explorer\n";
            text += "\n...\n..\n.";
            label1.Text = text;
            this.Height = this.Width = label1.Width + 4;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (label1.Location.Y + label1.Height > 10)
                label1.Location = new Point(label1.Location.X, label1.Location.Y - 1);
            else
                label1.Location = new Point(label1.Location.X, 0);
        }
    }
}
