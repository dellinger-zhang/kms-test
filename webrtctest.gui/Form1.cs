using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace webrtctest.gui
{
    public partial class Form1 : Form
    {
        class RunInfo
        {
            public string logPath;
            public Process process;
        }

        private static readonly IDictionary<string, RunInfo> processes = new Dictionary<string, RunInfo>();
        private static int max_clients = 1;
        private static readonly string curdir = Environment.CurrentDirectory;
        private static readonly string logdir = Path.Combine(curdir, "logs", DateTime.Now.ToString("yyyyMMddHHmmss"));
        private static int indicator = 0;
        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory(logdir);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            max_clients = Decimal.ToInt32(numericUpDown1.Value);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            for (int i = 1; i <= max_clients; i++)
            {
                Interlocked.Increment(ref indicator);
                worker.ReportProgress(i);
                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                string logname = Path.Combine(logdir, "client_" + indicator + ".log");
                p.StartInfo.FileName = "webrtctest.exe";
                p.StartInfo.Arguments = logname;
                
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.RedirectStandardError = false;
                lock (processes)
                {
                    processes.Add("client " + indicator, new RunInfo { process = p, logPath = logname });
                }

                p.Start();
                Thread.Sleep(500);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            lock(processes)
            {
                foreach (var p in processes)
                {
                    p.Value.process.Kill();
                    p.Value.process.Dispose();
                    Thread.Sleep(50);
                }
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = (string)listBox1.SelectedItem;
            key = key.Replace(" ", "_");
            string fileName = Path.Combine(logdir, key + ".log");

            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(fs);
                textBox1.Text = reader.ReadToEnd();
            }
            else
                textBox1.Text = "File not found:" + fileName;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            listBox1.Items.Add("client " + indicator);
        }
    }
}
