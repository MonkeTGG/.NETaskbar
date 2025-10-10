using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Windows10TB
{
    public partial class Form1 : Form
    {
        startMenu startMenu = new startMenu();
        //>=- importing -=<\\
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("shell32.dll")]
        static extern bool SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

        const int SW_RESTORE = 9;
        public const int ABM_NEW = 0x00000000;
        public const int ABM_REMOVE = 0x00000001;
        public const int ABM_QUERYPOS = 0x00000002;
        public const int ABM_SETPOS = 0x00000003;
        public const int ABM_GETSTATE = 0x00000004;
        public const int ABM_GETTASKBARPOS = 0x00000005;
        public const int ABM_ACTIVATE = 0x00000006;
        public const int ABM_GETAUTOHIDEBAR = 0x00000007;
        public const int ABM_SETAUTOHIDEBAR = 0x00000008;
        public const int ABM_WINDOWPOSCHANGED = 0x00000009;
        public const int ABM_SETSTATE = 0x0000000A;
        public const int ABE_LEFT = 0;
        public const int ABE_TOP = 1;
        public const int ABE_RIGHT = 2;
        public const int ABE_BOTTOM = 3;
        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public Form1()
        {
            InitializeComponent();
            this.TopMost = true;
            // this.Opacity = .90; //>=- make's the entire thing transparent and i don't want that -=<\\

            //>=- become an appbar pls -=<\\
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = this.Handle;
            abd.uEdge = ABE_BOTTOM;

            abd.rc.left = Screen.PrimaryScreen.Bounds.Left;
            abd.rc.right = Screen.PrimaryScreen.Bounds.Right;
            abd.rc.top = Screen.PrimaryScreen.Bounds.Bottom - 38;
            abd.rc.bottom = Screen.PrimaryScreen.Bounds.Bottom;

            SHAppBarMessage(ABM_NEW, ref abd);
            SHAppBarMessage(ABM_QUERYPOS, ref abd);
            SHAppBarMessage(ABM_SETPOS, ref abd);

            this.FormClosing += Form1_FormClosing;
            
        }
        bool AddAppsToTB(Process p)
        {
            Image img = Icon.ExtractAssociatedIcon(p.MainModule.FileName).ToBitmap();
            Button icon = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Size = new Size(33, 18),
                Location = new Point(0, 0),
                BackgroundImageLayout = ImageLayout.Zoom,
                BackgroundImage = img
            };
            icon.FlatAppearance.BorderSize = 0;
            icon.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            icon.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            icon.Margin = new Padding(0, 0, 10, 0);
            main.Controls.Add(icon);
            icon.Click += new EventHandler(icon_Click);
            void icon_Click(object sender, EventArgs e)
            {
                // MessageBox.Show("hello :D");
                ShowWindow(p.MainWindowHandle, SW_RESTORE);
                SetForegroundWindow(p.MainWindowHandle);
                
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Bottom - this.Size.Height);
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Right, Screen.PrimaryScreen.Bounds.Bottom - this.Location.Y);
            //>=- get all processes and add to TB -=<\\
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (!String.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        // MessageBox.Show(p.MainWindowTitle, "Found a process!");
                        AddAppsToTB(p);
                    }
                }
                catch
                {

                }
            };

            //>=- subscribe to process opening -=<\\
            //>=- happening soon™ -=<\\

            Taskbar.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("start menu", "your taskbar is talking to you fatty");
            /* if (startMenu.Visible)
            {
                startMenu.Hide();
            }
            startMenu.Show(); */
            SendKeys.Send("^{ESC}");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = this.Handle;
            SHAppBarMessage(ABM_REMOVE, ref abd);
            Taskbar.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^{ESC} {BACKSPACE}");
        }
    }
}

//>=- thank you stackoverflow <3 -=<\\

public class Taskbar
{
    [DllImport("user32.dll")]
    private static extern int FindWindow(string className, string windowText);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(int hwnd, int command);

    [DllImport("user32.dll")]
    public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

    [DllImport("user32.dll")]
    private static extern int GetDesktopWindow();

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 1;

    protected static int Handle
    {
        get
        {
            return FindWindow("Shell_TrayWnd", "");
        }
    }

    protected static int HandleOfStartButton
    {
        get
        {
            int handleOfDesktop = GetDesktopWindow();
            int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
            return handleOfStartButton;
        }
    }

    private Taskbar()
    {
        // hide ctor
    }

    public static void Show()
    {
        ShowWindow(Handle, SW_SHOW);
        ShowWindow(HandleOfStartButton, SW_SHOW);
    }

    public static void Hide()
    {
        ShowWindow(Handle, SW_HIDE);
        ShowWindow(HandleOfStartButton, SW_HIDE);
    }
}