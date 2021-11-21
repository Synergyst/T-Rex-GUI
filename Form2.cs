
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace TRexGUI {
    public partial class Form2 : Form {
        public Form2() {
            InitializeComponent();
            this.FormClosing += (s, e) => {
                if (Form1.actuallyClose == false) {
                    this.Hide();
                    e.Cancel = true;
                }
            };
        }
        public void ChooseFile(TextBox tb, OpenFileDialog ofd, String defaultFilename, String windowTitle) {
            ofd.Title = windowTitle;
            ofd.FileName = defaultFilename;
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == DialogResult.OK) {
                tb.Text = ofd.SafeFileName;
            }
        }
        private void Form2_Load(object sender, EventArgs e) {
            /*this.FormClosing += (s, e) => {
                this.Hide();
                e.Cancel = true;
            };*/
            if (Properties.Settings.Default.Maximised) {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Maximized;
                Size = Properties.Settings.Default.Size;
            } else if (Properties.Settings.Default.Minimised) {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Minimized;
                Size = Properties.Settings.Default.Size;
            } else {
                Location = Properties.Settings.Default.Location;
                Size = Properties.Settings.Default.Size;
            }
            String Config = "";
            if (string.IsNullOrEmpty(Properties.Settings.Default.Config)) {
                Console.WriteLine("No configuration file is set!\nYour default configuration filename is: config_example\n\n");
                Properties.Settings.Default.Config = "config_example";
                Config = Properties.Settings.Default.Config;
                textBox2.Text = Config;
            } else {
                Config = Properties.Settings.Default.Config;
                textBox2.Text = Config;
            }
            String ExeName = "";
            if (string.IsNullOrEmpty(Properties.Settings.Default.ExeName)) {
                Console.WriteLine("No binary filename is set!\nYour default binary filename is: t-rex.exe\n\n");
                Properties.Settings.Default.ExeName = "t-rex.exe";
                ExeName = Properties.Settings.Default.ExeName;
                textBox5.Text = ExeName;
            } else {
                ExeName = Properties.Settings.Default.ExeName;
                textBox5.Text = ExeName;
            }
        }
        private void Form2_FormClosing(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Maximized) {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximised = true;
                Properties.Settings.Default.Minimised = false;
            }
            else if (WindowState == FormWindowState.Normal) {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = false;
            } else {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = true;
            }
            Properties.Settings.Default.Config = textBox2.Text;
            Properties.Settings.Default.ExeName = textBox5.Text;
            Properties.Settings.Default.Save();
            Console.WriteLine("Configuration window hidden..");
        }
        private void button3_Click(object sender, EventArgs e) {
            ChooseFile(textBox2, openFileDialog1, "config_example", "Select the T-Rex configuration file");
            Properties.Settings.Default.Save();
        }
        private void GenerateNewAPIKey() {
            MessageBox.Show((TRexGUI.Program.ExecuteProcess(textBox5.Text, "--api-generate-key " + textBox4.Text + " --config " + textBox2.Text, AppDomain.CurrentDomain.BaseDirectory, ProcessPriorityClass.Normal, false)));
        }
        private void timer1_Tick_1(object sender, EventArgs e) {
            timer1.Stop();
            GenerateNewAPIKey();
        }
        private void textBox4_TextChanged(object sender, EventArgs e) {
            timer1.Stop();
            timer1.Start();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                textBox4.UseSystemPasswordChar = true;
            } else {
                textBox4.UseSystemPasswordChar = false;
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e) {
            timer3.Stop();
            timer3.Start();
        }
        private void timer3_Tick(object sender, EventArgs e) {
            timer3.Stop();
            Properties.Settings.Default.Config = textBox2.Text;
            Properties.Settings.Default.Save();
            String ConfigFullPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.Config;
            Console.WriteLine(File.Exists(ConfigFullPath) ? "Config filename exists." : "Config filename does not exist!");
        }
        private void button4_Click(object sender, EventArgs e) {
            ChooseFile(textBox5, openFileDialog2, "t-rex.exe", "Select the T-Rex Miner executable");
            Properties.Settings.Default.Save();
        }
        private void timer5_Tick(object sender, EventArgs e) {
            timer5.Stop();
            Properties.Settings.Default.ExeName = textBox5.Text;
            Properties.Settings.Default.Save();
            String ExeFullPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.ExeName;
            Console.WriteLine(File.Exists(ExeFullPath) ? "Executable filename exists." : "Executable filename does not exist!");
        }
        private void textBox5_TextChanged(object sender, EventArgs e) {
            timer5.Stop();
            timer5.Start();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // TODO: Obviously dynamically change this port number, as not everyone will use 4067.
            // Though those who are changing it probably have a reason and aren't even using this tool to help hand-hold them..
            System.Diagnostics.Process.Start("http://127.0.0.1:4067");
        }
        private void button1_Click(object sender, EventArgs e) {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to create/overwrite config.json with the contents of the config_example file?", "New configuration", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes) {
                String DefaultConfigFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_example");
                String ConfigFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                if ((File.Exists(DefaultConfigFullPath) && !File.Exists(ConfigFullPath))) {
                    Console.WriteLine("Can create config.json");
                    try {
                        // Will not overwrite if the destination file already exists.
                        // TODO: Question user if they want to restore a previous config.json[.bak] if found at this step since config.json does not exist
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_example"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                    }
                    // Catch exception if the file was already copied.
                    catch (IOException copyError) {
                        Console.WriteLine(copyError.Message);
                    }
                    Properties.Settings.Default.Config = "config.json";
                    textBox2.Text = "config.json";
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Now using config.json as your configuration file.\n");
                } else if ((File.Exists(DefaultConfigFullPath) && File.Exists(ConfigFullPath))) {
                    Console.WriteLine("config.json already exists, though can be backed up then recreated");
                    try {
                        // Will not overwrite if the destination file already exists.
                        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json.bak")))
                            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json.bak"));
                        File.Move(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json.bak"));
                        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_example"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                    }
                    // Catch exception if the file was already copied.
                    catch (IOException copyError) {
                        Console.WriteLine(copyError.Message);
                    }
                    Properties.Settings.Default.Config = "config.json";
                    textBox2.Text = "config.json";
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Now using config.json as your configuration file.");
                } else if (File.Exists(DefaultConfigFullPath)) {
                    MessageBox.Show("config.json can not be created as config_example does not exist!\n\nCan not create new configuration file!\nPlease make sure you extracted config_example from your T-Rex zip/tar.gz archive..");
                }
                /*Console.WriteLine(File.Exists(DefaultConfigFullPath) ? "Default config filename exists." : "Default config filename does not exist!");
                Console.WriteLine(File.Exists(ConfigFullPath) ? "Config filename exists." : "Config filename does not exist!");*/
            } else if (dialogResult == DialogResult.No) {
                //do something else
            }
        }
    }
}