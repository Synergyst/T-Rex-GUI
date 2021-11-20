using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Management;

namespace TRexGUI {
    static class Program {
        public class MultiFormContext : ApplicationContext {
            private int openForms;
            public MultiFormContext(params Form[] forms) {
                openForms = forms.Length;
                foreach (var form in forms) {
                    form.FormClosed += (s, args) => {
                        //When we have closed the last of the "starting" forms, 
                        //end the program.
                        if (Interlocked.Decrement(ref openForms) == 0)
                            ExitThread();
                    };
                    form.Show();
                }
            }
        }
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid) {
            // Cannot close 'system idle process'.
            if (pid == 0) {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc) {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            } try {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            } catch (ArgumentException) {
                // Process already exited.
            }
        }
        /// <summary>
        /// Execute external process.
        /// Block until process has terminated.
        /// Capture output.
        /// </summary>
        /// <param name="binaryFilename"></param>
        /// <param name="arguments"></param>
        /// <param name="currentDirectory"></param>
        /// <param name="priorityClass">Priority of started process.</param>
        /// <returns>stdout output.</returns>
        public static string ExecuteProcess(string binaryFilename, string arguments, string currentDirectory, ProcessPriorityClass priorityClass, bool dontShowWindow = true) {
            if (String.IsNullOrEmpty(binaryFilename)) {
                return "no command given.";
            }
            Process p = new Process();
            if (!String.IsNullOrEmpty(currentDirectory))
                p.StartInfo.WorkingDirectory = currentDirectory;
            p.StartInfo.FileName = currentDirectory + binaryFilename;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = dontShowWindow;
            p.Start();
            // Cannot set priority process is started.
            p.PriorityClass = priorityClass;
            // Must have the readToEnd BEFORE the WaitForExit(), to avoid a deadlock condition
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            //Console.Write(output);
            /*if (p.ExitCode != 0) {
                throw new Exception(String.Format("Process '{0} {1}' ExitCode was {2}", binaryFilename, arguments, p.ExitCode));
            }*/
            string standardError = p.StandardError.ReadToEnd();
            if (!String.IsNullOrEmpty(standardError)) {
                throw new Exception(String.Format("Process '{0} {1}' StandardError was {2}", binaryFilename, arguments, standardError));
            }
            return output;
        }
        // use console from another process
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AttachConsole(int procId);
        private const int ATTACH_PARENT_PROCESS = -1;
        [STAThread]
        public static void Main(string[] args) {
            // show GUI like
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MultiFormContext(new Form1(), new Form2()));
        }
    }
}