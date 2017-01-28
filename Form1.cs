using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DosToWin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DateTime StartTime = DateTime.Now;
        private void button1_Click(object sender, EventArgs e)
        {
            StartTime = DateTime.Now;
            timer1.Enabled = true;

        }

        System.Collections.Concurrent.ConcurrentQueue<string> Lst = null;
        void ProcPath()
        {
            string dPath = "";

            while (Lst.TryDequeue(out dPath))
            {
                var SaveFileName = Path.GetDirectoryName(dPath) + "\\" + Path.GetFileName(dPath) + ".csv";
                if (File.Exists(SaveFileName)) File.Delete(SaveFileName);

                var res = Loader.FromDBF(dPath, Encoding.Default);


                DataTableConverter.ToCSV(res, SaveFileName, Encoding.Default, ";", true);

            }

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            int FinishedTasks = 0;

            var listOfDbfs = Directory.GetFiles(textBox1.Text, "*.dbf", SearchOption.AllDirectories);
            Lst = new System.Collections.Concurrent.ConcurrentQueue<string>(listOfDbfs);
            //Lst = new System.Collections.Concurrent.ConcurrentQueue<string>(new string[] { @"D:\xxx\D2\95\DETAILS.DBF"});

            var AllTasks = new Task[] {

                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; }),
                Task.Factory.StartNew(ProcPath).ContinueWith((a) => { FinishedTasks++; })

            };

            Task.Factory.ContinueWhenAll(AllTasks, (a) => { MessageBox.Show("finished in " + (DateTime.Now - StartTime)); });

            //while (FinishedTasks < 6)
            //{
            //    System.Threading.Thread.CurrentThread.

            //}

            //int Total = listOfDbfs.Length;
            //int Current = 0;
            //foreach (var dbfPath in listOfDbfs)
            //{
            //    this.Text = (++Current).ToString() + " Of " + Total;

            //    var SaveFileName = Path.GetDirectoryName(dbfPath) + "\\" + Path.GetFileName(dbfPath) + ".csv";
            //    if (File.Exists(SaveFileName)) File.Delete(SaveFileName);

            //    var res = Loader.FromDBF(dbfPath, Encoding.Default);
            //    DataTableConverter.ToCSV(res, SaveFileName, Encoding.UTF8, ";", true);

            //}
             
        }


    }
}
