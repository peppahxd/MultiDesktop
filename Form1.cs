using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiDesktop
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int iKey);


        Manager Manager { get; set; }

        public Form1()
        {
            InitializeComponent();
            SetStartup();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Manager = new Manager();
            Manager.Init();

            

            System.Timers.Timer update = new System.Timers.Timer();
            update.Enabled = true;
            update.Interval = 1;
            update.Elapsed += (s, e) =>
            {
                Manager.Networker.Update();

                if (GetAsyncKeyState(0x2D) == -32767)
                {
                    Manager.KeyboardManager.bShareKeyboard = !Manager.KeyboardManager.bShareKeyboard;
                    Manager.MouseManager.bShareMouse = !Manager.MouseManager.bShareMouse;

                    label1.Text = Manager.KeyboardManager.bShareKeyboard.ToString();
                }
            };
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Manager.KeyboardManager.UnhookKeyboard() && !Manager.MouseManager.UnhookMouse())
                MessageBox.Show("failed to unhook");
        }

        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("MultiDesktop", @"C:\Users\Peppah\source\repos\MultiDesktop\bin\Debug\netcoreapp3.1\MultiDesktop.exe");

        }
    }
}
