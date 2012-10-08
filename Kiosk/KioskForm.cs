using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Kiosk
{
    public partial class KioskForm : Form
    {
        #region Properties

        private globalKeyboardHook GBHook;
        private bool ClosingAllowed = false;
        private bool AllreadyFocused = false;

        private string HomeAddress
        {
            get
            {
                return Properties.Settings.Default.HomeUrl;
            }
        }

        private string Password
        {
            get
            {
                return Properties.Settings.Default.Password;
            }
        }

        private bool AllowDownload
        {
            get
            {
                return Properties.Settings.Default.AllowDownload;
            }
        }

        private bool AllowFileInBrowser
        {
            get
            {
                return Properties.Settings.Default.AllowFileInBrowser;
            }
        }

        private int InactiveResetTime
        {
            get
            {
                return Properties.Settings.Default.InactiveResetTime;
            }
        }

        private string BlockedDownloadMessage
        {
            get
            {
                return Properties.Settings.Default.BlockedDownloadMessage;
            }
        }

        private List<String> _AllowedFileinBrowserContentTypes;

        private List<String> AllowedFileinBrowserContentTypes
        {
            get
            {
                if (_AllowedFileinBrowserContentTypes == null)
                {
                    _AllowedFileinBrowserContentTypes = new List<string>();
                    foreach (string contentType in Properties.Settings.Default.AllowFileInBrowserMimeTypes.Split(';'))
                    {
                        _AllowedFileinBrowserContentTypes.Add(contentType);
                    }
                }
                return _AllowedFileinBrowserContentTypes;
            }
        }

        private Stopwatch InactiveStopWatch = new Stopwatch();

        private WebBrowser WebBrowser;

        #endregion

        public KioskForm()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.ClosingAllowed == false)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void EnableKioskMode()
        {
            if (this.IsAdministrator() == false)
            {
                MessageBox.Show("This program can only run under an administrator account.");
                this.ClosingAllowed = true;
                this.Close();
                return;
            }

            //Init webbrowser and others
            this.InitControls();

            //Global keyboardhook
            this.GBHook = new globalKeyboardHook(Process.GetCurrentProcess(), this);

            this.ClosingAllowed = false;
            this.DisableTaskmanager();
            Taskbar.Hide();
            this.Startup(true);
        }

        public void DisableTaskmanager()
        {
            try
            {
                using (RegistryKey keyTaskManager = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (keyTaskManager == null)
                    {
                        using (RegistryKey subKeyLogOff = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies", true))
                        {
                            subKeyLogOff.CreateSubKey("System");
                        }
                    }
                }
                using (RegistryKey keyTaskManager = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    keyTaskManager.SetValue("DisableTaskMgr", 1);
                }
            }
            catch
            {
                MessageBox.Show("An error occured while disabling the task manager.");
            }
        }

        public void EnableTaskManager()
        {
            try
            {
                using (RegistryKey keyTaskManager = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    keyTaskManager.SetValue("DisableTaskMgr", 0);
                }
            }
            catch 
            {
                MessageBox.Show("En error occured while enabling the taskmanager.");
            }
        }

        public bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void InitControls()
        {
            this.WebBrowser = new WebBrowser();
            this.WebBrowser.Location = new Point(0, 36);
            this.WebBrowser.Size = new Size(this.Width, this.Height - 36);
            this.WebBrowser.Navigate(this.HomeAddress);

            this.WebBrowser.CanGoBackChanged += new EventHandler(WebBrowser_CanGoBackChanged);
            this.WebBrowser.CanGoForwardChanged += new EventHandler(WebBrowser_CanGoForwardChanged);
            this.WebBrowser.NewWindow += new CancelEventHandler(WebBrowser_NewWindow);
            this.WebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
            this.WebBrowser.Navigating += new WebBrowserNavigatingEventHandler(WebBrowser_Navigating);
            this.WebBrowser.AllowWebBrowserDrop = false;
            ((SHDocVw.WebBrowser)this.WebBrowser.ActiveXInstance).FileDownload += new SHDocVw.DWebBrowserEvents2_FileDownloadEventHandler(WebBrowser_FileDownload);

            this.Controls.Add(this.WebBrowser);

            this.tbUrl.Text = this.HomeAddress;
            this.tbUrl.Size = new Size((int)(this.Width * 0.75) - 117, this.tbUrl.Height);
        }

        void WebBrowser_FileDownload(bool activeDocument, ref bool cancel)
        {
            if (activeDocument == false)
            {
                cancel = true;
                MessageBox.Show(this.BlockedDownloadMessage);
            }
        }

        public void DisableKiosMode()
        {
            this.ClosingAllowed = true;

            try
            {
                this.GBHook.unhook();
                this.GBHook = null;

                this.EnableTaskManager();

                Taskbar.Show();

                this.Startup(false);
            }
            catch
            {
                MessageBox.Show("An error occured while restoring all settings.");
            }

            this.Close();
        }


        void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //Block local file opening.
            if (e.Url.AbsoluteUri.StartsWith(@"file:"))
            {
                e.Cancel = true;
            }

            //Special handler for files which should not be downloaded but openend in browser.
            if (e.Cancel == false && this.AllowFileInBrowser == true)
            {
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(e.Url);

                    //we only need the header part of http response
                    request.Method = "HEAD";

                    using (System.Net.WebResponse response = request.GetResponse())
                    {
                        //Allowed filetypes wich will be opend in browser.

                        string contentType = response.ContentType;

                        if (this.AllowedFileinBrowserContentTypes.Any(c => c.StartsWith(contentType)))
                        {
                            System.Net.WebRequest fileRequest = System.Net.WebRequest.Create(e.Url);
                            using (System.Net.WebResponse fileResponse = fileRequest.GetResponse())
                            {
                                System.IO.Stream s = fileResponse.GetResponseStream();
                                s.Position = 0;
                                this.WebBrowser.DocumentStream = s;
                            }
                        }
                    }
                }
                catch { }
            }

            InactiveStopWatch.Reset();
            InactiveStopWatch.Start();
        }

        string url;

        void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.WebBrowser.Document != null)
            {
                HtmlElementCollection links = this.WebBrowser.Document.Links;
                foreach (HtmlElement var in links)
                {
                    var.AttachEventHandler("onclick", LinkClicked);
                }
            }
        }

        private void LinkClicked(object sender, EventArgs e)
        {
            HtmlElement link = this.WebBrowser.Document.ActiveElement;
            url = link.GetAttribute("href");
            this.tbUrl.Text = url;
        }

        private void WebBrowser_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (string.IsNullOrEmpty(url) == false)
            {
                this.WebBrowser.Navigate(url);
            }
        }

        private void WebBrowser_CanGoForwardChanged(object sender, EventArgs e)
        {
            this.btnForward.Enabled = this.WebBrowser.CanGoForward;
        }

        private void WebBrowser_CanGoBackChanged(object sender, EventArgs e)
        {
            this.btnBack.Enabled = this.WebBrowser.CanGoBack;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.tbUrl.GotFocus += new EventHandler(tbUrl_GotFocus);
            this.EnableKioskMode();
            this.tmrCheckInactive.Start();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.WebBrowser.Navigate(this.tbUrl.Text);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.WebBrowser.GoBack();
            this.tbUrl.Text = this.WebBrowser.Url.AbsoluteUri;
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            this.WebBrowser.GoForward();
            this.tbUrl.Text = this.WebBrowser.Url.AbsoluteUri;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            this.NavigateToHome();
        }

        private void NavigateToHome()
        {
            //Clear all session data.
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);

            this.tbUrl.Text = this.HomeAddress;
            this.WebBrowser.Navigate(this.HomeAddress);    
        }

        #region Url textbox focus handlers

        private void tbUrl_Enter(object sender, EventArgs e)
        {
            this.tbUrl.SelectAll();
        }

        private void tbUrl_Leave(object sender, EventArgs e)
        {
            this.AllreadyFocused = false;
        }

        private void tbUrl_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.AllreadyFocused == false && this.tbUrl.SelectionLength == 0)
            {
                this.AllreadyFocused = true;
                this.tbUrl.SelectAll();
            }
        }

        private void tbUrl_GotFocus(object sender, EventArgs e)
        {
            if (MouseButtons == MouseButtons.None)
            {
                this.tbUrl.SelectAll();
                this.AllreadyFocused = true;
            }
        }

        #endregion 

        private void Startup(bool add)
        {
            using (RegistryKey keyExplorer = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true))
            {
                if (keyExplorer == null)
                {
                    using (RegistryKey subKeyLogOff = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies", true))
                    {
                        subKeyLogOff.CreateSubKey("Explorer");
                    }
                }
            }

            using (RegistryKey keyAppRun = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            using (RegistryKey keyExplorer = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true))
            using (RegistryKey keySystemCU = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true))
            using (RegistryKey keySystemLM = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true))
            {
                if (add)
                {
                    keyAppRun.SetValue("Kiosk", System.Reflection.Assembly.GetEntryAssembly().Location);

                    keySystemLM.SetValue("HideFastUserSwitching", 1);
                    keySystemCU.SetValue("DisableLockWorkstation", 1);
                    keySystemCU.SetValue("DisableChangePassword", 1);
                    keyExplorer.SetValue("NoLogoff", 1);
                    keyExplorer.SetValue("NoClose", 1);
                }
                else
                {
                    keyExplorer.SetValue("NoLogoff", 0);
                    keyExplorer.SetValue("NoClose", 0);
                    keySystemLM.SetValue("HideFastUserSwitching", 0);

                    if (keySystemCU != null)
                    {
                        keySystemCU.SetValue("DisableLockWorkstation", 0);
                        keySystemCU.SetValue("DisableChangePassword", 0);
                    }
                }
            }
        }

        private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        private void tmrCheckInactive_Tick(object sender, EventArgs e)
        {
            if (this.InactiveStopWatch.Elapsed.TotalMinutes >= this.InactiveResetTime && this.tbUrl.Text != this.HomeAddress)
            {
                this.NavigateToHome();
            }
        }

        /// <summary>
        /// Show the admin pannel in a new thread so the keyboard isn't locked up.
        /// </summary>
        internal void ShowAdmin()
        {
            BackgroundWorker b = new BackgroundWorker();
            b.RunWorkerCompleted += new RunWorkerCompletedEventHandler(b_RunWorkerCompleted);
            b.RunWorkerAsync();
        }

        void b_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowAdminThread();
        }

        private void ShowAdminThread()
        {
            InputForm i = new InputForm(this.Password);
            if (i.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.DisableKiosMode();
                using (RegistryKey keyAppRun = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    keyAppRun.DeleteValue("Kiosk");
                }
            }
        }
    }
}
