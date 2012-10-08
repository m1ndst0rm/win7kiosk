namespace Kiosk
{
    partial class KioskForm
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
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnForward = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.tmrCheckInactive = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.tbUrl.Location = new System.Drawing.Point(117, 8);
            this.tbUrl.Name = "textBox1";
            this.tbUrl.Size = new System.Drawing.Size(469, 20);
            this.tbUrl.TabIndex = 0;
            this.tbUrl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyUp);
            this.tbUrl.Leave += new System.EventHandler(this.tbUrl_Leave);
            this.tbUrl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tbUrl_MouseUp);
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.Transparent;
            this.btnHome.Image = global::Kiosk.Properties.Resources.home;
            this.btnHome.Location = new System.Drawing.Point(79, 1);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(32, 33);
            this.btnHome.TabIndex = 3;
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnForward
            // 
            this.btnForward.BackColor = System.Drawing.Color.Transparent;
            this.btnForward.Enabled = false;
            this.btnForward.Image = global::Kiosk.Properties.Resources.ie_forward;
            this.btnForward.Location = new System.Drawing.Point(41, 1);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(32, 33);
            this.btnForward.TabIndex = 2;
            this.btnForward.UseVisualStyleBackColor = false;
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            this.btnBack.Enabled = false;
            this.btnBack.Image = global::Kiosk.Properties.Resources.ie_back;
            this.btnBack.Location = new System.Drawing.Point(3, 1);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(32, 33);
            this.btnBack.TabIndex = 1;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // tmrCheckInactive
            // 
            this.tmrCheckInactive.Enabled = true;
            this.tmrCheckInactive.Interval = 1000;
            this.tmrCheckInactive.Tick += new System.EventHandler(this.tmrCheckInactive_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 417);
            this.Controls.Add(this.btnHome);
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.tbUrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Timer tmrCheckInactive;
    }
}

