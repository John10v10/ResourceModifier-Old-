using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceModifier
{
    public class FileNode : TreeNode
    {
        private File _f;
        public File file
        {
            get { return _f; }
            set
            {
                _f = value;
                _f.SetNode(this);
                Nodes.Clear();
                _f.BuildNodes();
            }
        }
        public FileNode(File f)
        {
            _f = f;
            f.SetNode(this);
            f.BuildNodes();
        }
    }
    public unsafe class File
    {
        private FileNode node;
        [Browsable(false)]
        private UInt32 _size = 0;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        public virtual UInt64 Size { get { return _size; } }
        private string _fn;
        [Category("General File Information")]
        [Description("Name of the selected file.")]
        [DisplayName("File Name")]
        public virtual string FileName { get { updateNodeName(_fn); return _fn; } set { updateNodeName(_fn = value); } }

        public string updateNodeName(string fn)
        {
            if (node != null) node.Text = fn;
            return fn;
        }

        public void SetNode(FileNode node)
        {
            this.node = node;
        }
        public FileNode GetNode()
        {
            if (node == null)
            {
                Console.Write("File " + FileName + " does not have a node yet.");
            }
            return node;
        }

        public void SetSize(UInt32 s)
        {
            _size = s;
        }

        public virtual void Load(byte* data)
        {
            Console.Write("File data loaded.");
        }
        
        public virtual byte[] Save()
        {
            return new byte[Size];
        }

        public virtual void BuildNodes()
        {
            updateNodeName(FileName);
        }
    }
}
