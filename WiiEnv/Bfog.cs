using ResourceModifier;
using ResourceModifier.CommonTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WiiEnv
{
    public unsafe class FOGD : File
    {
        public enum FalloffType : byte
        {
            NoEffect = 0,
            Linear1 = 1,
            Linear2 = 2,
            Linear3 = 3,
            Logarithmic = 4,
            Squared = 5,
            InvertedSquare = 6,
            Ease = 7
        }
        struct Header
        {
            public const uint Tag = 0x44474F46;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[7];
            public bfloat start, end, unk1, unk2;
            public RGBAPixel color;
            public byte ft;
            public byte ebd;
            fixed byte pad2[10];
        }
        public static bool IsImportable() => false;

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Fog Element Information")]
        [Description("Version of the selected wii fog file.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Fog Range")]
        [Description("This is where in depth the fog starts to takes effect.")]
        public float Start { get { return _header.start; } set { _header.start = value; } }
        [Category("Wii Fog Range")]
        [Description("This is where in depth the fog ends at full effect.")]
        public float End { get { return _header.end; } set { _header.end = value; } }
        [Category("Wii Fog Element Unknown Information")]
        [DisplayName("Unknown 1")]
        public float Unknown1 { get { return _header.unk1; } set { _header.unk1 = value; } }
        [Category("Wii Fog Element Unknown Information")]
        [DisplayName("Unknown 2")]
        public float Unknown2 { get { return _header.unk2; } set { _header.unk2 = value; } }
        [Category("Wii Fog Element Information")]
        [Description("Color of the fog.")]
        public ARGBPixel Color { get { return _header.color; } set { _header.color = value; } }
        [Category("Wii Fog Element Unknown Information")]
        [DisplayName("Unknown 3")]
        public byte Unknown3 { get { return (byte)(_header.ft>>3); } set { _header.ft = (byte)((_header.ft % (1 << 3)) | (value << 3)); } }
        [Category("Wii Fog Range")]
        [Description("The type of curve that determines the shape of the fog's effect.")]
        [DisplayName("Falloff Type")]
        public FalloffType FT { get { return (FalloffType)(_header.ft % (1 << 3)); }
            set { _header.ft = (byte)((_header.ft & ~((1 << 3)-1)) | (byte)value); } }
        [Category("Wii Fog Element Information")]
        [Description("Does this fog element even cause effects at all?")]
        public bool Enabled { get { return _header.ebd != 0; } set { _header.ebd = (byte)(value?1:0); } }

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii fog element: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            ResourceModifier.Console.Write("Wii fog element loaded: " + FileName + ".");
        }
        public override byte[] Save()
        {
            _header._tag = Header.Tag;
            _header.size = 0x30;
            byte[] output = new byte[Size];
            fixed (byte* o = &output[0])
            fixed (Header* h = &_header)
                Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
            return output;
        }
    }
    public unsafe class Bfog : File
    {
        struct Header
        {
            public const uint Tag = 0x4D474F46;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[7];
            public bushort nE;
            fixed byte pad2[2];
        }
        public static bool IsImportable() => true;
        public static string GetTypeName() => "Wii Fog";
        public static string GetExtension() => "*.bfog";

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Fog Information")]
        [Description("Version of the selected wii fog file.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Fog Information")]
        [Description("Number of elements.")]
        [DisplayName("Element Count")]
        public UInt16 ElementCount { get { return _header.nE; } }

        private TreeNode ElementListNode;

        [XmlArrayItem("Element")]
        public List<FOGD> Elements;

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii fog: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            if (ElementCount > 0)
            {
                for (int i = 0; i < ElementCount; ++i)
                {
                    FOGD element = new FOGD();
                    element.FileName = "Element " + i;
                    element.Load(data + 0x14 + (i * 0x30));
                    Elements.Add(element);
                }
            }
            ResourceModifier.Console.Write("Wii fog loaded: " + FileName + ".");
        }

        public override void BuildNodes()
        {
            ElementListNode = new TreeNode("Elements");
            foreach (FOGD e in Elements)
            {
                ElementListNode.Nodes.Add(new FileNode(e));
            }
            GetNode().Nodes.Add(ElementListNode);
            base.BuildNodes();
        }

        public override byte[] Save()
        {
            _header._tag = Header.Tag;
            _header.size = (buint)sizeof(Header);
            _header.nE = 0;
            List<byte[]> FOGDDataList = new List<byte[]>();
            foreach (FOGD e in Elements)
            {
                byte[] FOGDData = e.Save();
                FOGDDataList.Add(FOGDData);
                _header.size += (buint)FOGDData.Length;
                ++_header.nE;
            }
            byte[] output = new byte[_header.size];
            fixed (byte* o = &output[0])
            {
                fixed (Header* h = &_header)
                {
                    Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
                }
                byte* seek = o + sizeof(Header);
                foreach (byte[] FOGDData in FOGDDataList)
                {
                    fixed (byte* i = &FOGDData[0])
                        Buffer.MemoryCopy(i, seek, FOGDData.Length, FOGDData.Length);
                    seek += FOGDData.Length;
                }
            }
            return output;
        }
        public Bfog()
        {
            Elements = new List<FOGD>();
        }
    }
}
