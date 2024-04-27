using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;

namespace CommandsBox
{
    class RawDataBuilder
    {
        public RawDataBuilder()
        {
            this.memoryStream = new MemoryStream();
            this.writer = new BinaryWriter(this.memoryStream);
            this.writer.BaseStream.Position = 3L;
        }
        public RawDataBuilder(PacketTypes type)
        {
            this.memoryStream = new MemoryStream();
            this.writer = new BinaryWriter(this.memoryStream);
            this.writer.BaseStream.Position = 3L;
            SetType(type);
        }

        public RawDataBuilder SetType(PacketTypes type)
        {
            this.type = (int)type;
            long position = this.writer.BaseStream.Position;
            this.writer.BaseStream.Position = 2L;
            this.writer.Write((short)type);
            this.writer.BaseStream.Position = position;
            return this;
        }

        public RawDataBuilder PackSByte(sbyte num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackByte(byte num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackInt16(short num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt16(ushort num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackInt32(int num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt32(uint num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt64(ulong num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackSingle(float num)
        {
            this.writer.Write(num);
            return this;
        }

        public RawDataBuilder PackString(string str)
        {
            this.writer.Write(str);
            return this;
        }

        public RawDataBuilder PackRGB(Color? color)
        {
            this.writer.WriteRGB((Color)color);
            return this;
        }
        public RawDataBuilder PackVector2(Vector2 v)
        {
            this.writer.Write(v.X);
            this.writer.Write(v.Y);
            return this;
        }

        private void UpdateLength()
        {
            long position = this.writer.BaseStream.Position;
            this.writer.BaseStream.Position = 0L;
            this.writer.Write((short)position);
            this.writer.BaseStream.Position = position;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

        public byte[] GetByteData()
        {
            this.UpdateLength();
            return this.memoryStream.ToArray();
        }

        int type = -1;

        public MemoryStream memoryStream;

        public BinaryWriter writer;
    }
}