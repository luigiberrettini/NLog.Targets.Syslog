using System;
using System.IO;
using System.Text;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    internal class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private const int MaxBufferCapacity = int.MaxValue;
        private static readonly byte[] LineFeedBytes = { 0x0A };
        private static readonly int MaxOctetCountLength;
        private static readonly byte[] InitialOctetCount;
        private readonly MemoryStream memoryStream;
        private readonly FramingMethod? framingMethod;

        public int Length => (int)memoryStream.Length;

        public int EmptyContentLength => framingMethod == FramingMethod.OctetCounting ? InitialOctetCount.Length : 0;

        public int SendOffset
        {
            get
            {
                if (framingMethod != FramingMethod.OctetCounting)
                    return 0;

                var contentLength = Length - EmptyContentLength;
                var octetCount = contentLength.ToString();
                return MaxOctetCountLength - octetCount.Length;
            }
        }

        static ByteArray()
        {
            MaxOctetCountLength = int.MaxValue.ToString().Length;
            InitialOctetCount = new byte[MaxOctetCountLength + 1];
        }

        public ByteArray(long initialCapacity, FramingMethod? framing = null)
        {
            var capacity = EnforceAllowedValues(initialCapacity);
            memoryStream = new MemoryStream(capacity);
            framingMethod = framing;
        }

        public static implicit operator byte[](ByteArray byteArray)
        {
            return byteArray.memoryStream.GetBuffer();
        }

        public byte this[int index]
        {
            get
            {
                if (index >= Length)
                    throw new IndexOutOfRangeException();

                return memoryStream.GetBuffer()[index];
            }
        }

        public void Append(byte[] buffer)
        {
            if (buffer.Length == 0)
                return;

            memoryStream.Write(buffer, 0, buffer.Length);
        }

        public void PrepareForSend()
        {
            if (!framingMethod.HasValue)
                return;

            if (framingMethod == FramingMethod.NonTransparent)
            {
                Append(LineFeedBytes);
                return;
            }

            var contentLength = Length - EmptyContentLength;
            var octetCount = contentLength.ToString();
            var leftPaddedOctetCount = octetCount.PadLeft(MaxOctetCountLength);
            var prefix = new ASCIIEncoding().GetBytes($"{leftPaddedOctetCount} ");
            var buffer = memoryStream.GetBuffer();
            Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
        }

        public void Reset()
        {
            memoryStream.SetLength(Zero);
            if (framingMethod == FramingMethod.OctetCounting)
                memoryStream.Write(InitialOctetCount, 0, InitialOctetCount.Length);
        }

        public void Resize(long newLength)
        {
            if (memoryStream.Length != newLength)
                memoryStream.SetLength(newLength);
        }

        private static int EnforceAllowedValues(long initialCapacity)
        {
            if (initialCapacity <= 0)
                return DefaultBufferCapacity;
            if (initialCapacity > MaxBufferCapacity)
                return MaxBufferCapacity;
            return (int)initialCapacity;
        }

        public void Dispose()
        {
            memoryStream.SetLength(Zero);
            memoryStream.Capacity = Zero;
            memoryStream.Dispose();
        }
    }
}