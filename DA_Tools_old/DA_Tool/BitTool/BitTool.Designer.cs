namespace DA_Tool.BitTool
{
    partial class BitTool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.grp7Bit = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGenDJBHash = new System.Windows.Forms.Button();
            this.txtDJBHashOutput = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDJBHashInput = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.grp7Bit.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(9, 33);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(9, 115);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 59);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Decompress";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(149, 59);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "Compress";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(135, 115);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 20);
            this.textBox4.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Output";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(135, 33);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(132, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Input";
            // 
            // grp7Bit
            // 
            this.grp7Bit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grp7Bit.Controls.Add(this.label1);
            this.grp7Bit.Controls.Add(this.button2);
            this.grp7Bit.Controls.Add(this.textBox1);
            this.grp7Bit.Controls.Add(this.textBox4);
            this.grp7Bit.Controls.Add(this.label2);
            this.grp7Bit.Controls.Add(this.label3);
            this.grp7Bit.Controls.Add(this.textBox2);
            this.grp7Bit.Controls.Add(this.textBox3);
            this.grp7Bit.Controls.Add(this.button1);
            this.grp7Bit.Controls.Add(this.label4);
            this.grp7Bit.Location = new System.Drawing.Point(12, 12);
            this.grp7Bit.Name = "grp7Bit";
            this.grp7Bit.Size = new System.Drawing.Size(410, 153);
            this.grp7Bit.TabIndex = 11;
            this.grp7Bit.TabStop = false;
            this.grp7Bit.Text = "7 Bit / 8-Bit";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnGenDJBHash);
            this.groupBox1.Controls.Add(this.txtDJBHashOutput);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtDJBHashInput);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 171);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 133);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DJB2 Hash";
            // 
            // btnGenDJBHash
            // 
            this.btnGenDJBHash.Location = new System.Drawing.Point(9, 59);
            this.btnGenDJBHash.Name = "btnGenDJBHash";
            this.btnGenDJBHash.Size = new System.Drawing.Size(100, 23);
            this.btnGenDJBHash.TabIndex = 14;
            this.btnGenDJBHash.Text = "Generate Hash";
            this.btnGenDJBHash.UseVisualStyleBackColor = true;
            this.btnGenDJBHash.Click += new System.EventHandler(this.btnGenDJBHash_Click);
            // 
            // txtDJBHashOutput
            // 
            this.txtDJBHashOutput.Location = new System.Drawing.Point(9, 101);
            this.txtDJBHashOutput.Name = "txtDJBHashOutput";
            this.txtDJBHashOutput.ReadOnly = true;
            this.txtDJBHashOutput.Size = new System.Drawing.Size(226, 20);
            this.txtDJBHashOutput.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 85);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Output";
            // 
            // txtDJBHashInput
            // 
            this.txtDJBHashInput.Location = new System.Drawing.Point(9, 33);
            this.txtDJBHashInput.Name = "txtDJBHashInput";
            this.txtDJBHashInput.Size = new System.Drawing.Size(226, 20);
            this.txtDJBHashInput.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Input";
            // 
            // BitTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 324);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grp7Bit);
            this.Name = "BitTool";
            this.Text = "Bit Tool";
            this.grp7Bit.ResumeLayout(false);
            this.grp7Bit.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grp7Bit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGenDJBHash;
        private System.Windows.Forms.TextBox txtDJBHashOutput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDJBHashInput;
        private System.Windows.Forms.Label label6;
    }
}