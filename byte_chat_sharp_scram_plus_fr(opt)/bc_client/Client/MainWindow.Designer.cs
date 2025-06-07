namespace Client
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.SendButton = new System.Windows.Forms.Button();
            this.NickBox = new System.Windows.Forms.TextBox();
            this.labelNick = new System.Windows.Forms.Label();
            this.labelDialog = new System.Windows.Forms.Label();
            this.ConnectToChatButton = new System.Windows.Forms.Button();
            this.labelInput = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InputMessages = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.Settings = new System.Windows.Forms.ToolStripMenuItem();
            this.Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowMessages = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputPasswdBox = new System.Windows.Forms.TextBox();
            this.RegisterButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            resources.ApplyResources(this.SendButton, "SendButton");
            this.SendButton.BackColor = System.Drawing.Color.White;
            this.SendButton.ForeColor = System.Drawing.Color.Black;
            this.SendButton.Image = global::Client.Properties.Resources.concrete_1646788__340;
            this.SendButton.Name = "SendButton";
            this.SendButton.UseVisualStyleBackColor = false;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // NickBox
            // 
            this.NickBox.AcceptsReturn = true;
            resources.ApplyResources(this.NickBox, "NickBox");
            this.NickBox.BackColor = System.Drawing.Color.Honeydew;
            this.NickBox.Name = "NickBox";
            // 
            // labelNick
            // 
            resources.ApplyResources(this.labelNick, "labelNick");
            this.labelNick.BackColor = System.Drawing.SystemColors.WindowText;
            this.labelNick.ForeColor = System.Drawing.Color.White;
            this.labelNick.Name = "labelNick";
            // 
            // labelDialog
            // 
            resources.ApplyResources(this.labelDialog, "labelDialog");
            this.labelDialog.Name = "labelDialog";
            // 
            // ConnectToChatButton
            // 
            resources.ApplyResources(this.ConnectToChatButton, "ConnectToChatButton");
            this.ConnectToChatButton.BackColor = System.Drawing.Color.White;
            this.ConnectToChatButton.ForeColor = System.Drawing.Color.Black;
            this.ConnectToChatButton.Image = global::Client.Properties.Resources.concrete_1646788__340;
            this.ConnectToChatButton.Name = "ConnectToChatButton";
            this.ConnectToChatButton.UseVisualStyleBackColor = false;
            this.ConnectToChatButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // labelInput
            // 
            resources.ApplyResources(this.labelInput, "labelInput");
            this.labelInput.Name = "labelInput";
            // 
            // contextMenuStrip1
            // 
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            // 
            // InputMessages
            // 
            resources.ApplyResources(this.InputMessages, "InputMessages");
            this.InputMessages.BackColor = System.Drawing.Color.Honeydew;
            this.InputMessages.Name = "InputMessages";
            this.InputMessages.KeyUp += new System.Windows.Forms.KeyEventHandler(this.EnterSending);
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // Menu
            // 
            resources.ApplyResources(this.Menu, "Menu");
            this.Menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Settings,
            this.Exit});
            this.Menu.Name = "Menu";
            // 
            // Settings
            // 
            resources.ApplyResources(this.Settings, "Settings");
            this.Settings.Name = "Settings";
            this.Settings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // Exit
            // 
            resources.ApplyResources(this.Exit, "Exit");
            this.Exit.Name = "Exit";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // ShowMessages
            // 
            resources.ApplyResources(this.ShowMessages, "ShowMessages");
            this.ShowMessages.BackColor = System.Drawing.Color.Honeydew;
            this.ShowMessages.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShowMessages.Name = "ShowMessages";
            this.ShowMessages.ReadOnly = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.SystemColors.WindowText;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Name = "label1";
            // 
            // InputPasswdBox
            // 
            this.InputPasswdBox.AcceptsReturn = true;
            resources.ApplyResources(this.InputPasswdBox, "InputPasswdBox");
            this.InputPasswdBox.BackColor = System.Drawing.Color.Honeydew;
            this.InputPasswdBox.Name = "InputPasswdBox";
            // 
            // RegisterButton
            // 
            resources.ApplyResources(this.RegisterButton, "RegisterButton");
            this.RegisterButton.BackColor = System.Drawing.Color.White;
            this.RegisterButton.ForeColor = System.Drawing.Color.Black;
            this.RegisterButton.Image = global::Client.Properties.Resources.concrete_1646788__340;
            this.RegisterButton.Name = "RegisterButton";
            this.RegisterButton.UseVisualStyleBackColor = false;
            this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
            // 
            // Main
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Client.Properties.Resources._5a43720d4bb70382ba6e4b212b450c62;
            this.Controls.Add(this.RegisterButton);
            this.Controls.Add(this.InputPasswdBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ShowMessages);
            this.Controls.Add(this.InputMessages);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.ConnectToChatButton);
            this.Controls.Add(this.labelDialog);
            this.Controls.Add(this.labelNick);
            this.Controls.Add(this.NickBox);
            this.Controls.Add(this.SendButton);
            this.ForeColor = System.Drawing.Color.Transparent;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.TextBox NickBox;
        private System.Windows.Forms.Label labelNick;
        private System.Windows.Forms.Label labelDialog;
        private System.Windows.Forms.Button ConnectToChatButton;
        private System.Windows.Forms.Label labelInput;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.RichTextBox InputMessages;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem Menu;
        private System.Windows.Forms.ToolStripMenuItem Settings;
        private System.Windows.Forms.ToolStripMenuItem Exit;
        private System.Windows.Forms.RichTextBox ShowMessages;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InputPasswdBox;
        private System.Windows.Forms.Button RegisterButton;
    }
}

