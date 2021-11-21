using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace TRexGUI {
    public partial class Form1 : Form {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        private const int WM_VSCROLL = 277;
        private const int SB_PAGEBOTTOM = 7;
        public static bool actuallyClose = false;
        internal static void ScrollToBottom(RichTextBox richTextBox) {
            SendMessage(richTextBox.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
            richTextBox.SelectionStart = richTextBox.Text.Length;
        }
        public Form1() {
            InitializeComponent();
            this.FormClosing += (s, e) => {
                if (Form1.actuallyClose == false) {
                    notifyIcon1.Visible = true;
                    notifyIcon1.ShowBalloonTip(1500);
                    this.ShowInTaskbar = false;
                    this.Hide();
                    e.Cancel = true;
                } else {
                    notifyIcon1.Visible = false;
                }
            };
        }
        private object syncGate = new object();
        private Process process;
        private StringBuilder output = new StringBuilder();
        private bool outputChanged;
        private void button1_Click_1(object sender, EventArgs e) {
            lock (syncGate) {
                if (process != null) return;
            }
            output.Clear();
            outputChanged = false;
            richTextBox1.Text = "";
            System.Windows.Forms.Form f2 = System.Windows.Forms.Application.OpenForms["Form2"];
            String ExeFullPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.ExeName;
            String ConfigFullPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.Config;
            if (File.Exists(ExeFullPath) && File.Exists(ConfigFullPath)) {
                process = new Process();
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                process.StartInfo.FileName = ((Form2)f2).textBox5.Text;
                String exeParams = @"--config " + ((Form2)f2).textBox2.Text + @" ";
                process.StartInfo.Arguments = exeParams;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.Exited += new EventHandler(process_HasExited);
                process.Start();
                new Thread(ReadData) { IsBackground = true }.Start();
            } else {
                richTextBox1.Text = "Executable file is missing!\n\nPlease open the Configuration GUI and follow the steps here: https://github.com/Synergyst/T-Rex-GUI";
                return;
            }
        }
        private static void process_HasExited(object sender, System.EventArgs e) {
            Console.WriteLine("T-Rex process has exited..");
        }
        private void ReadData() {
            var input = process.StandardOutput;
            int nextChar;
            while ((nextChar = input.Read()) >= 0) {
                lock (syncGate) {
                    output.Append((char)nextChar);
                    if (!outputChanged) {
                        outputChanged = true;
                        BeginInvoke(new Action(OnOutputChanged));
                    }
                }
            }
            lock (syncGate) {
                process.Dispose();
                process = null;
            }
        }
        private void OnOutputChanged() {
            lock (syncGate) {
                richTextBox1.Text = output.ToString();
                if (checkBox1.Checked) {
                    ScrollToBottom(richTextBox1);
                }
                outputChanged = false;
            }
        }
        private void button2_Click(object sender, EventArgs e) {
            lock (syncGate) {
                actuallyClose = true;
                if (process != null) {
                    richTextBox1.Text = output.ToString() + "Killing T-Rex process..";
                    Console.WriteLine("Killing T-Rex process..");
                    if (checkBox1.Checked) {
                        ScrollToBottom(richTextBox1);
                    }
                    process.Kill();
                } else {
                    Console.WriteLine("T-Rex process already stopped..");
                }
                for (int x = 0; x < Application.OpenForms.Count; x++) {
                    if (Application.OpenForms[x] != this)
                        Application.OpenForms[x].Close();
                }
                Console.WriteLine("Closed other windows(s), exiting now..");
                outputChanged = false;
                System.Threading.Thread.Sleep(2500);
                this.Close();
            }
        }
        private void button3_Click(object sender, EventArgs e) {
            lock (syncGate) {
                if (process != null) {
                    process.Kill();
                    richTextBox1.Text = output.ToString() + "Killed T-Rex process with exit code: " + process.ExitCode;
                    if (checkBox1.Checked)
                    {
                        ScrollToBottom(richTextBox1);
                    }
                    outputChanged = false;

                }
            }
        }
        private void button4_Click(object sender, EventArgs e) {
            System.Windows.Forms.Form f2 = System.Windows.Forms.Application.OpenForms["Form2"];
            ((Form2)f2).Show();
            Console.WriteLine("Configuration window reopened..");
        }
        private void Form1_Resize(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized) {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(2500);
                this.ShowInTaskbar = false;
                this.Hide();
            } else if (this.WindowState == FormWindowState.Normal) {
                //this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            Show();
            //this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            //notifyIcon1.Visible = false;
        }
        private void Form1_Load(object sender, EventArgs e) {
            //
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            //
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // TODO: Obviously dynamically change this port number, as not everyone will use 4067.
            // Though those who are changing it probably have a reason and aren't even using this tool to help hand-hold them..
            System.Diagnostics.Process.Start("http://127.0.0.1:4067");
        }
    }
}