using ResourceModifier;
using ResourceModifier.CommonTypes;
using ResourceModifier.MathFuncs;
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
    public unsafe class LTEXEntry : File
    {
        struct Header
        {
            public bfloat ity;
            public byte spr, enb;
            fixed byte pad[2];
        }

        public static bool IsImportable() => false;

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return 8; } }
        [Category("Wii Light Texture Entry Information")]
        [Description("Strength of how much the light is affecting this texture.")]
        public float Intensity { get { return _header.ity; } set { _header.ity = value; } }
        [Category("Wii Light Texture Entry Information")]
        [Description("How much light gets spread across the sphere in the texture.")]
        public float Spread
        {
            get
            {
                int I = _header.spr;
                if (I > 6) I -= 14;
                return Map.Range(-7f, 6f, 1.0f, 0.0f, I);
            }
            set
            {
                int I = (int)Math.Round(Map.Range(1.0f, 0.0f, -7f, 6f, value));
                if (I < 0) I += 14;
                _header.spr = (byte)I;
            }
        }
        [Category("Wii Light Texture Entry Information")]
        [Description("Ignores this light if this is false.")]
        public bool Enabled { get { return _header.enb != 0; } set { _header.enb = (byte)(value ? 1 : 0); } }

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii light texture entry: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            ResourceModifier.Console.Write("Wii light texture entry loaded: " + FileName + ".");
        }
        public override byte[] Save()
        {
            byte[] output = new byte[Size];
            fixed (byte* o = &output[0])
            fixed (Header* h = &_header)
                Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
            return output;
        }
    }
    public unsafe class LTEX : File
    {
        struct Header
        {
            public const uint Tag = 0x5845544C;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[7];
            public bushort nLET;
            public byte amb;
            public byte ambid;
            public fixed byte tn[0x28];
            public bfloat ambIty;
            fixed byte pad2[8];
        }

        public static bool IsImportable() => false;

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Light Texture Information")]
        [Description("Version of the selected wii light texture.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Light Texture Information")]
        [Description("Number of textures.")]
        [DisplayName("Texture Count")]
        public UInt16 TextureCount { get { return _header.nLET; } }
        [Category("Wii Light Texture Ambient")]
        [Description("Determines whether this texture uses ambient lighting or not.")]
        [DisplayName("Enable Ambient Lighting")]
        public byte EnableAmbient { get { return _header.amb; } set { _header.amb = value; } }
        [Category("Wii Light Texture Ambient")]
        [Description("Specifies which ambient light to use for this texture.")]
        [DisplayName("Ambient Light ID")]
        public byte AmbientID { get { return _header.ambid; } set { _header.ambid = value; } }
        [Category("Wii Light Texture Information")]
        [Description("Name of the selected file.")]
        [DisplayName("File Name")]
        public override string FileName
        {
            get
            {
                int i = 0;
                for (; i < 0x28; ++i)
                {
                    if (_header.tn[i] == 0) break;
                }
                fixed (byte* tn = _header.tn) return updateNodeName(Encoding.UTF8.GetString(tn, i));
            }
            set
            {
                byte[] newname = Encoding.UTF8.GetBytes(updateNodeName(value));
                for (int i = 0; i < 0x27; ++i)
                {
                    _header.tn[i] = (newname.Length > i) ? newname[i] : (byte)0;
                }
            }
        }
        [Category("Wii Light Texture Ambient")]
        [Description("Specifies how bright the ambient light will affect this texture.")]
        [DisplayName("Ambient Light Intensity")]
        public float AmbientIntensity { get { return _header.ambIty; } set { _header.ambIty = value; } }
        
        private TreeNode EntryListNode;

        [XmlArrayItem("Entry")]
        public List<LTEXEntry> Entries;

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii light texture: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            if (TextureCount > 0)
            {
                for (int i = 0; i < TextureCount; ++i)
                {
                    LTEXEntry entry = new LTEXEntry();
                    entry.FileName = "Entry " + i;
                    entry.Load(data + 0x48 + (i * 0x8));
                    Entries.Add(entry);
                }
            }
            ResourceModifier.Console.Write("Wii light texture loaded: " + FileName + ".");
        }

        public override byte[] Save()
        {
            _header._tag = Header.Tag;
            _header.size = (buint)sizeof(Header);
            _header.nLET = 0;
            List<byte[]> LTEXEntryDataList = new List<byte[]>();
            foreach (LTEXEntry entry in Entries)
            {
                byte[] LTEXEntryData = entry.Save();
                LTEXEntryDataList.Add(LTEXEntryData);
                _header.size += (buint)LTEXEntryData.Length;
                ++_header.nLET;
            }
            byte[] output = new byte[_header.size];
            fixed (byte* o = &output[0])
            {
                fixed (Header* h = &_header)
                {
                    Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
                }
                byte* seek = o + sizeof(Header);
                foreach (byte[] LTEXEntryData in LTEXEntryDataList)
                {
                    fixed (byte* i = &LTEXEntryData[0])
                        Buffer.MemoryCopy(i, seek, LTEXEntryData.Length, LTEXEntryData.Length);
                    seek += LTEXEntryData.Length;
                }
            }
            return output;
        }
        public override void BuildNodes()
        {
            EntryListNode = new TreeNode("Entries");
            foreach (LTEXEntry lightEntry in Entries)
            {
                EntryListNode.Nodes.Add(new FileNode(lightEntry));
            }
            GetNode().Nodes.Add(EntryListNode);
            base.BuildNodes();
        }
        public LTEX()
        {
            Entries = new List<LTEXEntry>();
            _header.ver = 2;
        }
    }
    public unsafe class Blmap : File
    {
        struct Header
        {
            public const uint Tag = 0x50414D4C;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[7];
            public bushort nTex;
            fixed byte pad2[14];
        }
        public static bool IsImportable() => true;
        public static string GetTypeName() => "Wii Light Map";
        public static string GetExtension() => "*.blmap";

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Light Map Information")]
        [Description("Version of the selected wii light file.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Light Map Information")]
        [Description("Number of textures.")]
        [DisplayName("Texture Count")]
        public UInt16 TextureCount { get { return _header.nTex; } }

        private TreeNode LightTextureListNode;

        [XmlArrayItem("LightTexture")]
        public List<LTEX> LightTextures;

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii light map: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            if (TextureCount > 0)
            {
                ulong _seeker = 0x20;
                for (int i = 0; i < TextureCount; ++i)
                {
                    LTEX lightTexture = new LTEX();
                    lightTexture.Load(data + _seeker);
                    _seeker += lightTexture.Size;
                    LightTextures.Add(lightTexture);
                }
            }
            ResourceModifier.Console.Write("Wii light map loaded: " + FileName + ".");
        }

        public override void BuildNodes()
        {
            LightTextureListNode = new TreeNode("Light Textures");
            foreach (LTEX lightTexture in LightTextures)
            {
                LightTextureListNode.Nodes.Add(new FileNode(lightTexture));
            }
            GetNode().Nodes.Add(LightTextureListNode);
            base.BuildNodes();
        }

        public override byte[] Save()
        {
            _header._tag = Header.Tag;
            _header.size = (buint)sizeof(Header);
            _header.nTex = 0;
            List<byte[]> LTEXDataList = new List<byte[]>();
            foreach (LTEX lightTexture in LightTextures)
            {
                byte[] LTEXData = lightTexture.Save();
                LTEXDataList.Add(LTEXData);
                _header.size += (buint)LTEXData.Length;
                ++_header.nTex;
            }
            byte[] output = new byte[_header.size];
            fixed (byte* o = &output[0])
            {
                fixed (Header* h = &_header)
                {
                    Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
                }
                byte* seek = o + sizeof(Header);
                foreach (byte[] LTEXData in LTEXDataList)
                {
                    fixed (byte* i = &LTEXData[0])
                        Buffer.MemoryCopy(i, seek, LTEXData.Length, LTEXData.Length);
                    seek += LTEXData.Length;
                }
            }
            return output;
        }

        public Blmap()
        {
            LightTextures = new List<LTEX>();
            _header.ver = 0;
        }
    }
}
