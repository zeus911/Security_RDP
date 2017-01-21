using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Net.Configuration;
using System.Reflection;

namespace Security_RDP
{
    public partial class frmMain : Form
    {

        private const int EM_SETCUEBANNER = 0x1501;

        private Regex ipPattern = new Regex(@"(([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})+)");
        private Match match;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);
        

        public frmMain()
        {
            InitializeComponent();
        }

        private void SystemEvents_Session(object sender, EventArgs e)
        {
            try
            {
                const string LogName = "Microsoft-Windows-TerminalServices-RemoteConnectionManager/Operational";
                EventLogQuery query = new EventLogQuery(LogName, PathType.LogName, "*[System/Level=4][System/EventID=1149]");
                query.ReverseDirection = true;
                EventLogReader logReader = new EventLogReader(query);

                EventRecord eventInstance = logReader.ReadEvent();
                match = ipPattern.Match(eventInstance.FormatDescription());
                if (match.Success)
                {
                    if (!textBox1.Text.Equals(match.Value))
                    {
                        /** Events that occur when IPv4 does not match. **/
                    }
                }
               
            } catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_Session);
            if (!ToggleAllowUnsafeHeaderParsing(true))
            {
                MessageBox.Show("error");
                Application.Exit();
            }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            SendMessage(txtMailTo.Handle, EM_SETCUEBANNER, 0, "user@domain.com");
        }

        
        /**
         * 
         *  HttpWebRequest() Allow
         * 
         * */
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {

            notifyIcon1.BalloonTipTitle = "Security :: RDP";
            notifyIcon1.BalloonTipText = "Always monitor in tray mode.";

            notifyIcon1.ShowBalloonTip(3);
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
