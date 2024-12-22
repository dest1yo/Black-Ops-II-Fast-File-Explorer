using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackOps2Explorer
{
    // Ref: https://stackoverflow.com/questions/8620885/c-sharp-binary-reader-in-big-endian
    public class EndiannessAwareBinaryReader : BinaryReader
    {
        public enum Endianness
        {
            Little,
            Big,
        }

        private Endianness _endianness = Endianness.Little;

        public EndiannessAwareBinaryReader(Stream input) : base(input)
        {
        }

        public EndiannessAwareBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public EndiannessAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public EndiannessAwareBinaryReader(Stream input, Endianness endianness) : base(input)
        {
            _endianness = endianness;
        }

        public EndiannessAwareBinaryReader(Stream input, Encoding encoding, Endianness endianness) : base(input, encoding)
        {
            _endianness = endianness;
        }

        public EndiannessAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness) : base(input, encoding, leaveOpen)
        {
            _endianness = endianness;
        }

        public void SetEndianness(Endianness endianness)
        {
            _endianness = endianness;
        }

        public override short ReadInt16() => ReadInt16(_endianness);

        public override int ReadInt32() => ReadInt32(_endianness);

        public override long ReadInt64() => ReadInt64(_endianness);

        public override ushort ReadUInt16() => ReadUInt16(_endianness);

        public override uint ReadUInt32() => ReadUInt32(_endianness);

        public override ulong ReadUInt64() => ReadUInt64(_endianness);

        public short ReadInt16(Endianness endianness) => BitConverter.ToInt16(ReadForEndianness(sizeof(short), endianness), 0);

        public int ReadInt32(Endianness endianness) => BitConverter.ToInt32(ReadForEndianness(sizeof(int), endianness), 0);

        public long ReadInt64(Endianness endianness) => BitConverter.ToInt64(ReadForEndianness(sizeof(long), endianness), 0);

        public ushort ReadUInt16(Endianness endianness) => BitConverter.ToUInt16(ReadForEndianness(sizeof(ushort), endianness), 0);

        public uint ReadUInt32(Endianness endianness) => BitConverter.ToUInt32(ReadForEndianness(sizeof(uint), endianness), 0);

        public ulong ReadUInt64(Endianness endianness) => BitConverter.ToUInt64(ReadForEndianness(sizeof(ulong), endianness), 0);

        private byte[] ReadForEndianness(int bytesToRead, Endianness endianness)
        {
            var bytesRead = ReadBytes(bytesToRead);

            if ((endianness == Endianness.Little && !BitConverter.IsLittleEndian)
                || (endianness == Endianness.Big && BitConverter.IsLittleEndian))
            {
                Array.Reverse(bytesRead);
            }

            return bytesRead;
        }
    }

    public class MemoryUtil
    {
        public static UInt32 ReverseEndianUInt32(UInt32 value)
        {
            return ((value & 0x000000FF) << 24) |
                   ((value & 0x0000FF00) << 8) |
                   ((value & 0x00FF0000) >> 8) |
                   ((value & 0xFF000000) >> 24);
        }
    }
}
