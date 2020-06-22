using ResourceModifier;
using ResourceModifier.CommonTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace WiiEnv
{

    public unsafe class LOBJ : File
    {
        public enum LightType : byte
        {
            Unused = 0,
            Point2D = 1,
            Point3D = 2,
            FixedFront = 3,
            FixedBack = 4
        }
        struct Header
        {
            public const uint Tag = 0x4A424F4C;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[3];
            public buint unk1;
            public bushort unk2;
            public byte type;
            public byte unk3;
            public bushort kcl;
            public bushort unk4;
            public BVec3 org;
            public BVec3 dst;
            public bfloat ity;
            public RGBAPixel color;
            public buint unk5;
            public bfloat unk6;
            public bfloat unk7;
            public bfloat unk8;
            public blong unk9;
        }

        public static bool IsImportable() => false;


        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Light Object Information")]
        [Description("Version of the selected wii light object.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 1")]
        public UInt32 Unknown1 { get { return _header.unk1; } set { _header.unk1 = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 2")]
        public UInt16 Unknown2 { get { return _header.unk2; } set { _header.unk2 = value; } }
        [Category("Wii Light Object Information")]
        [DisplayName("Light Type")]
        [Description("Type of the selected wii light object.")]
        public LightType TypeOfLight { get { return (LightType)_header.type; } set { _header.type = (byte)value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 3")]
        public byte Unknown3 { get { return _header.unk3; } set { _header.unk3 = value; } }
        [Category("Wii Light Object Information")]
        public UInt16 KCL { get { return _header.kcl; } set { _header.kcl = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 4")]
        [TypeConverter(typeof(UInt16HexTypeConverter))]

        public UInt16 Unknown4 { get { return _header.unk4; } set { _header.unk4 = value; } }
        [Category("Wii Light Object Coords")]
        [Description("The position of the light.")]
        [TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Origin { get { return _header.org; } set { _header.org = value; } }
        [Category("Wii Light Object Coords")]
        [Description("The target aim the light is pointing at.")]
        [TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Destination { get { return _header.dst; } set { _header.dst = value; } }
        [Category("Wii Light Object Colors")]
        [Description("The brightness of the light.")]
        public float Intensity { get { return _header.ity; } set { _header.ity = value; } }
        [Category("Wii Light Object Colors")]
        [Description("The Color of the light.")]
        public RGBAPixel Color { get { return _header.color; } set { _header.color = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 5")]
        [TypeConverter(typeof(UInt32HexTypeConverter))]
        public UInt32 Unknown5 { get { return _header.unk5; } set { _header.unk5 = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 6")]
        public float Unknown6 { get { return _header.unk6; } set { _header.unk6 = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 7")]
        public float Unknown7 { get { return _header.unk7; } set { _header.unk7 = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 8")]
        public float Unknown8 { get { return _header.unk8; } set { _header.unk8 = value; } }
        [Category("Wii Light Object Unknown Information")]
        [DisplayName("Unknown 9")]
        [TypeConverter(typeof(Int64HexTypeConverter))]
        public long Unknown9 { get { return _header.unk9; } set { _header.unk9 = value; } }

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii light object: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            ResourceModifier.Console.Write("Wii light object loaded: " + FileName + ".");
        }
        public override byte[] Save()
        {
            _header._tag = Header.Tag;
            byte[] output = new byte[_header.size];
            fixed (byte* o = &output[0])
            fixed (Header* h = &_header)
                Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
            return output;
        }

        public LOBJ()
        {
            _header.size = 0x50;
            _header.ver = 2;
        }
    }
    public unsafe class AmbientLight : File
    {
        struct Header
        {
            public RGBAPixel color;
            public UInt32 unk;
        }

        public static bool IsImportable() => false;


        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return 8; } }

        [Category("Ambient Light Information")]
        [Description("Color of the selected ambient light.")]
        public RGBAPixel Color { get { return _header.color; } set { _header.color = value; } }

        [Category("Ambient Light Information")]
        [DisplayName("Unknown")]
        [TypeConverter(typeof(UInt32HexTypeConverter))]
        public UInt32 Unknown { get { return _header.unk; } set { _header.unk = value; } }

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii ambient light: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            ResourceModifier.Console.Write("Wii ambient light loaded: " + FileName + ".");
        }
        public override byte[] Save()
        {
            byte[] output = new byte[8];
            fixed (byte* o = &output[0])
                fixed (Header* h = &_header)
                    Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
            return output;
        }
        public AmbientLight()
        {

        }

    }
    public unsafe class Blight : File
    {
        struct Header
        {
            public const uint Tag = 0x5448474C;
            public uint _tag;
            public buint size;
            public byte ver;
            fixed byte pad1[3];
            public buint unk1;
            public bushort nLOBJ;
            public bushort nALs;
            public byte unk2, unk3, unk4, unk5;
            fixed byte pad2[16];
        }
        public static bool IsImportable() => true;
        public static string GetTypeName() => "Wii Light Data";
        public static string GetExtension() => "*.blight";

        private Header _header;
        [Category("General File Information")]
        [Description("Size of the selected file.")]
        [DisplayName("File Size")]
        [TypeConverter(typeof(UInt64HexTypeConverter))]
        public override UInt64 Size { get { return _header.size; } }
        [Category("Wii Light File Information")]
        [Description("Version of the selected wii light file.")]
        public byte Version { get { return _header.ver; } }
        [Category("Wii Light Unknown Information")]
        [DisplayName("Unknown 1")]
        public UInt32 Unknown1 { get { return _header.unk1; } set { _header.unk1 = value; } }
        [Category("Wii Light File Information")]
        [Description("Number of light objects.")]
        [DisplayName("Light Object Count")]
        public UInt16 LightObjectCount { get { return _header.nLOBJ; } }
        [Category("Wii Light File Information")]
        [Description("Number of ambient lights.")]
        [DisplayName("Ambient Light Count")]
        public UInt16 AmbientLightCount { get { return _header.nALs; } }
        [Category("Wii Light Unknown Information")]
        [DisplayName("Unknown 2")]
        public byte Unknown2 { get { return _header.unk2; } set { _header.unk2 = value; } }
        [Category("Wii Light Unknown Information")]
        [DisplayName("Unknown 3")]
        public byte Unknown3 { get { return _header.unk3; } set { _header.unk3 = value; } }
        [Category("Wii Light Unknown Information")]
        [DisplayName("Unknown 4")]
        public byte Unknown4 { get { return _header.unk4; } set { _header.unk4 = value; } }
        [Category("Wii Light Unknown Information")]
        [DisplayName("Unknown 5")]
        public byte Unknown5 { get { return _header.unk5; } set { _header.unk5 = value; } }

        private TreeNode LightObjectListNode;
        private TreeNode AmbientLightListNode;

        [XmlArrayItem("LightObject")]
        public List<LOBJ> LightObjects;
        [XmlArrayItem("AmbientLight")]
        public List<AmbientLight> AmbientLights;

        public override void Load(byte* data)
        {
            ResourceModifier.Console.Write("Loading wii light data: " + FileName + "...");
            fixed (Header* _h = &_header) Buffer.MemoryCopy(data, _h, sizeof(Header), sizeof(Header));
            if (LightObjectCount > 0)
            {
                for (int i = 0; i < LightObjectCount; ++i)
                {
                    LOBJ lightObject = new LOBJ();
                    lightObject.FileName = "Light " + i;
                    lightObject.Load(data + 0x28 + (i * 0x50));
                    LightObjects.Add(lightObject);
                }
            }
            if (AmbientLightCount > 0)
            {
                for (int i = 0; i < AmbientLightCount; ++i)
                {
                    AmbientLight ambientLight = new AmbientLight();
                    ambientLight.FileName = "Ambient Light " + i;
                    ambientLight.Load(data + 0x28 + (LightObjectCount * 0x50) + (i * 0x8));
                    AmbientLights.Add(ambientLight);
                }
            }
            ResourceModifier.Console.Write("Wii light data loaded: " + FileName + ".");
        }

        public override void BuildNodes()
        {
            LightObjectListNode = new TreeNode("Light Objects");
            foreach (LOBJ lightObject in LightObjects)
            {
                LightObjectListNode.Nodes.Add(new FileNode(lightObject));
            }
            AmbientLightListNode = new TreeNode("Ambient Lights");
            foreach (AmbientLight ambLight in AmbientLights)
            {
                AmbientLightListNode.Nodes.Add(new FileNode(ambLight));
            }
            GetNode().Nodes.Add(LightObjectListNode);
            GetNode().Nodes.Add(AmbientLightListNode);

            base.BuildNodes();
        }

        public override byte[] Save(){
            _header._tag = Header.Tag;
            _header.size = (buint)sizeof(Header);
            _header.nALs = _header.nLOBJ = 0;
            List<byte[]> LOBJDataList = new List<byte[]>();
            foreach (LOBJ lightObject in LightObjects)
            {
                byte[] LOBJData = lightObject.Save();
                LOBJDataList.Add(LOBJData);
                _header.size += (buint)LOBJData.Length;
                ++_header.nLOBJ;
            }
            List<byte[]> ALDataList = new List<byte[]>();
            foreach (AmbientLight ambLight in AmbientLights)
            {
                byte[] ALData = ambLight.Save(); 
                ALDataList.Add(ALData);
                _header.size += (buint)ALData.Length;
                ++_header.nALs;
            }
            byte[] output = new byte[_header.size];
            fixed (byte* o = &output[0]) {
                fixed (Header* h = &_header)
                    Buffer.MemoryCopy((byte*)h, o, sizeof(Header), sizeof(Header));
                byte* seek = o + sizeof(Header);
                foreach (byte[] LOBJData in LOBJDataList)
                {
                    fixed (byte* i = &LOBJData[0])
                        Buffer.MemoryCopy(i, seek, LOBJData.Length, LOBJData.Length);
                    seek += LOBJData.Length;
                }
                foreach (byte[] ALData in ALDataList)
                {
                    fixed (byte* i = &ALData[0])
                        Buffer.MemoryCopy(i, seek, ALData.Length, ALData.Length);
                    seek += ALData.Length;
                }
            }
            return output;
        }

        public Blight()
        {
            LightObjects = new List<LOBJ>();
            AmbientLights = new List<AmbientLight>();
            _header.ver = 2;
        }
    }
}
