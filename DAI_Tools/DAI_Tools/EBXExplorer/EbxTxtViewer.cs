using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;
using DAI_Tools.Search;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxTextViewer : UserControl
    {
        private EbxDataContainers currentFile = null;
        
        public EbxTextViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            disableSearch();
            this.Visible = false;
        }

        private void findButton_Click(object sender, EventArgs e)
        {
            search(findTextBox.Text);
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
           if (ebxFile != null)
           {
                currentFile = EbxDataContainers.fromDAIEbx(ebxFile, newStatus => {});
                render();
           }
        }

        public void render()
        {
            if (Visible)
            {
                if (currentFile != null)
                {
                    var xml = currentFile.toText();

                    if (xml.Length > 0)
                    {
                        rtb1.Text = xml;
                        findButton.Enabled = true;
                    }
                    else
                        disableSearch();
                }
            }
        }

        public void search(String what)
        {
            if (rtb1.TextLength > 0)
            {
                var cursorPos = rtb1.SelectionStart;

                rtb1.SelectAll();
                rtb1.SelectionBackColor = Color.White;

                int matchCount = 0;
                var lastMatchStart = 0;

                while (lastMatchStart >= 0 && lastMatchStart + 1 < rtb1.TextLength)
                {
                    lastMatchStart = rtb1.Find(what, lastMatchStart + 1, -1, 0);

                    if (lastMatchStart >= 0)
                    {
                        rtb1.SelectionBackColor = Color.Yellow;
                        matchCount += 1;
                    }
                }

                rtb1.SelectionStart = cursorPos;
                rtb1.SelectionLength = 0;

                matchesCountLabel.Visible = true;
                matchesCountLabel.Text = "Found " + matchCount + " matches";
            }
        }

        private void disableSearch()
        {
            matchesCountLabel.Visible = false;
            findButton.Enabled = false;
            findTextBox.Clear();
        }

        private void EbxTreeViewer_VisibleChanged(object sender, EventArgs e)
        {
            render();
        }
    }
}
