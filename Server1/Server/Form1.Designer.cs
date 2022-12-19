namespace Server
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
            this.connectButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.clientPort = new System.Windows.Forms.TextBox();
            this.listenButton = new System.Windows.Forms.Button();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.button_connectServer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ipAdress = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(26, 255);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(155, 64);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect To The Master Server";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Port:";
            // 
            // clientPort
            // 
            this.clientPort.Location = new System.Drawing.Point(69, 42);
            this.clientPort.Name = "clientPort";
            this.clientPort.Size = new System.Drawing.Size(112, 26);
            this.clientPort.TabIndex = 6;
            // 
            // listenButton
            // 
            this.listenButton.Location = new System.Drawing.Point(69, 89);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(84, 29);
            this.listenButton.TabIndex = 7;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.listenButton_Click);
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(210, 12);
            this.logs.Name = "logs";
            this.logs.ReadOnly = true;
            this.logs.Size = new System.Drawing.Size(702, 477);
            this.logs.TabIndex = 9;
            this.logs.Text = "";
            // 
            // button_connectServer
            // 
            this.button_connectServer.Location = new System.Drawing.Point(26, 362);
            this.button_connectServer.Name = "button_connectServer";
            this.button_connectServer.Size = new System.Drawing.Size(155, 64);
            this.button_connectServer.TabIndex = 10;
            this.button_connectServer.Text = "Connect To The Other Server";
            this.button_connectServer.UseVisualStyleBackColor = true;
            this.button_connectServer.Click += new System.EventHandler(this.button_connectServer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP:";
            // 
            // ipAdress
            // 
            this.ipAdress.Location = new System.Drawing.Point(69, 187);
            this.ipAdress.Name = "ipAdress";
            this.ipAdress.Size = new System.Drawing.Size(112, 26);
            this.ipAdress.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 503);
            this.Controls.Add(this.button_connectServer);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.listenButton);
            this.Controls.Add(this.clientPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.ipAdress);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox clientPort;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Button button_connectServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ipAdress;
    }
}

