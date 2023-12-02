using Bogus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HDF5CSharp.Winforms.Tests
{
    public partial class Form1 : Form
    {
        private string filename = "TestMemory.h5";
        private List<HDF5DataClass> Data { get; set; }
        private PerformanceCounter PC { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            Hdf5.Settings.EnableH5InternalErrorReporting(true);
            PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = Process.GetCurrentProcess().ProcessName;
        }

        private List<HDF5DataClass> ReadFile(bool compare)
        {
            var fileID = Hdf5.OpenFile(filename);
            List<HDF5DataClass> read = new List<HDF5DataClass>();
            int i = 0;
            bool readOK = true;
            do
            {
                var dataClass = Hdf5.ReadObject<HDF5DataClass>(fileID, $"testObject{i++}");
                if (dataClass != null)
                {
                   read.Add(dataClass);
                }
                else
                {
                    readOK = false;
                }
            }
            while (readOK);

            if (cbPopup.Checked)
            {
                MessageBox.Show($"After Read before close file: {Convert.ToInt32(PC.NextValue()) / 1024 / 1024}");
            }
            Hdf5.CloseFile(fileID);
            if (cbPopup.Checked)
            {
                MessageBox.Show($"After Read after close file: {Convert.ToInt32(PC.NextValue()) / 1024 / 1024}");
            }

            if (compare)
            {
                return read;
            }
            read.Clear();
            return new List<HDF5DataClass>();
        }

        private void btnCreateData_Click(object sender, EventArgs e)
        {
            if (cbPopup.Checked)
            {
                MessageBox.Show($"Before Data Creation: {Convert.ToInt32(PC.NextValue()) / 1024 / 1024}");
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            var fileID = Hdf5.CreateFile(filename);
            Data = new List<HDF5DataClass>();
            Randomizer.Seed = new Random(8675309);
            var random = new Randomizer();
            for (int i = 0; i < 3000; i++)
            {
                Data.Add(new HDF5DataClass
                {
                    Location = i * 10,
                    Image = random.Bytes(30000),
                });
            }

            if (cbPopup.Checked)
            {
                MessageBox.Show($"Before write Data: {Convert.ToInt32(PC.NextValue()) / 1024 / 1024}");
            }

            for (int i = 0; i < Data.Count; i++)
            {
                Hdf5.WriteObject(fileID, Data[i], $"testObject{i}");
            }
            Hdf5.CloseFile(fileID);
            if (cbPopup.Checked)
            {
                MessageBox.Show($"After Data Creation: {Convert.ToInt32(PC.NextValue()) / 1024 / 1024}");
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            var result = ReadFile(ceCompare.Checked);
            if (cbPopup.Checked)
            {
                MessageBox.Show($"After Read exit read method file: {Convert.ToInt32(PC.NextValue() / 1024) / 1024}");
            }

            if (ceCompare.Checked)
            {
                if (!Data.SequenceEqual(result))
                {
                    MessageBox.Show($"Not The same object:");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Text = $"Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + " [MB]"}";
        }
    }
}