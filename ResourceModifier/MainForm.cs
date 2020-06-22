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
using System.Xml;
using System.Xml.Serialization;

namespace ResourceModifier
{
    public partial class MainForm : Form
    {
        File RootFile;

        public MainForm()
        {
            InitializeComponent();
            TB.Text = Console.prelog;
            Console.Target = TB;
        }
        
        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AboutForm ab = new AboutForm();
            ab.ShowDialog();
        }

        private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RootFile = new File();
            RootFile.FileName = "New File";
            PG.SelectedObject = RootFile;
            TV.Nodes.Clear();
            TV.Nodes.Add(new FileNode(RootFile));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.LoadExternalAssemblies();
        }

        private unsafe void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "All Files|*";
            foreach (Type type in Program.ExternalTypes)
            {
                fileDialog.Filter += "|" + type.GetMethod("GetTypeName").Invoke(null, null) + "|" + type.GetMethod("GetExtension").Invoke(null, null);
            }
            if (fileDialog.ShowDialog() != DialogResult.OK) return;
            string path = fileDialog.FileName;
            if (!System.IO.File.Exists(path)) return;
            byte[] FileBytes = System.IO.File.ReadAllBytes(path);
            if (fileDialog.FilterIndex >= 2)
            {
                Type selectedType = (Program.ExternalTypes[fileDialog.FilterIndex - 2]);
                RootFile = Activator.CreateInstance(selectedType) as File;
            }
            else RootFile = new File();
            RootFile.SetSize((UInt32)FileBytes.Length);
            RootFile.FileName = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);

            fixed (byte* fb = &FileBytes[0])
            {
                RootFile.Load(fb);
            }

            TV.Nodes.Clear();
            TV.Nodes.Add(new FileNode(RootFile));

            PG.SelectedObject = RootFile;
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RootFile == null) return;

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = ((File)RootFile).FileName;
            fileDialog.Filter = "Xml Files|*.xml";
            if (fileDialog.ShowDialog() != DialogResult.OK) return;
            string path = fileDialog.FileName;
            XmlSerializer xs = new XmlSerializer(RootFile.GetType());

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings { Indent = true }))
                {
                    xs.Serialize(writer, RootFile);
                    System.IO.File.WriteAllText(path, sww.ToString()); // Your XML
                }
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PG.SelectedObject == null) return;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Xml Files|*.xml";
            if (fileDialog.ShowDialog() != DialogResult.OK) return;
            string path = fileDialog.FileName;
            if (!System.IO.File.Exists(path)) return;
            using (StreamReader s = new StreamReader(path))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(RootFile.GetType());
                    PG.SelectedObject = RootFile = (RootFile).GetNode().file = (File)xs.Deserialize(s);
                }
                catch(Exception ex)
                {
                    Console.Write(ex, Color.Red);
                }
            }
        }

        private void TV_Click(object sender, EventArgs e)
        {

        }

        private void TV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node is FileNode)) return;
            PG.SelectedObject = (e.Node as FileNode).file;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RootFile == null) return;
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (RootFile.GetType() == typeof(File)) fileDialog.Filter = "All Files|*";
            else fileDialog.Filter = RootFile.GetType().GetMethod("GetTypeName").Invoke(null, null) + "|" + RootFile.GetType().GetMethod("GetExtension").Invoke(null, null);
            if (fileDialog.ShowDialog() != DialogResult.OK) return;
            string path = fileDialog.FileName;
            System.IO.File.WriteAllBytes(path, RootFile.Save());
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TV.Nodes.Clear();
            PG.SelectedObject = RootFile = null;
        }
    }
}