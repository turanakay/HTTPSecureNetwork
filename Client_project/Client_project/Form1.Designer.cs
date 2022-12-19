namespace Client_project
{
    partial class Form1
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
            this.button_connect = new System.Windows.Forms.Button();
            this.button_upload = new System.Windows.Forms.Button();
            this.IP = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_disconnect = new System.Windows.Forms.Button();
            this.button_download = new System.Windows.Forms.Button();
            this.textBox_download = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(60, 119);
            this.button_connect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(112, 35);
            this.button_connect.TabIndex = 0;
            this.button_connect.Text = "Connect";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
            // 
            // button_upload
            // 
            this.button_upload.Enabled = false;
            this.button_upload.Location = new System.Drawing.Point(60, 251);
            this.button_upload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_upload.Name = "button_upload";
            this.button_upload.Size = new System.Drawing.Size(112, 35);
            this.button_upload.TabIndex = 1;
            this.button_upload.Text = "Upload";
            this.button_upload.UseVisualStyleBackColor = true;
            this.button_upload.Click += new System.EventHandler(this.button_upload_Click);
            // 
            // IP
            // 
            this.IP.Location = new System.Drawing.Point(60, 32);
            this.IP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(140, 26);
            this.IP.TabIndex = 2;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(60, 72);
            this.port.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(140, 26);
            this.port.TabIndex = 3;
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(231, 34);
            this.logs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.logs.Name = "logs";
            this.logs.ReadOnly = true;
            this.logs.Size = new System.Drawing.Size(818, 333);
            this.logs.TabIndex = 4;
            this.logs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Port:";
            // 
            // button_disconnect
            // 
            this.button_disconnect.Enabled = false;
            this.button_disconnect.Location = new System.Drawing.Point(60, 179);
            this.button_disconnect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_disconnect.Name = "button_disconnect";
            this.button_disconnect.Size = new System.Drawing.Size(112, 35);
            this.button_disconnect.TabIndex = 7;
            this.button_disconnect.Text = "Disconnect";
            this.button_disconnect.UseVisualStyleBackColor = true;
            this.button_disconnect.Click += new System.EventHandler(this.button_disconnect_Click);
            // 
            // button_download
            // 
            this.button_download.Enabled = false;
            this.button_download.Location = new System.Drawing.Point(60, 377);
            this.button_download.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_download.Name = "button_download";
            this.button_download.Size = new System.Drawing.Size(112, 35);
            this.button_download.TabIndex = 8;
            this.button_download.Text = "Download";
            this.button_download.UseVisualStyleBackColor = true;
            this.button_download.Click += new System.EventHandler(this.button_download_Click);
            // 
            // textBox_download
            // 
            this.textBox_download.Location = new System.Drawing.Point(60, 324);
            this.textBox_download.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_download.Name = "textBox_download";
            this.textBox_download.Size = new System.Drawing.Size(140, 26);
            this.textBox_download.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 473);
            this.Controls.Add(this.textBox_download);
            this.Controls.Add(this.button_download);
            this.Controls.Add(this.button_disconnect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.port);
            this.Controls.Add(this.IP);
            this.Controls.Add(this.button_upload);
            this.Controls.Add(this.button_connect);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.Button button_upload;
        private System.Windows.Forms.TextBox IP;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_disconnect;
        private System.Windows.Forms.Button button_download;
        private System.Windows.Forms.TextBox textBox_download;
    }
}

