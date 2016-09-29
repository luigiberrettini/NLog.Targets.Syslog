using System;
using System.IO;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog
{
    internal class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private const int MaxBufferCapacity = int.MaxValue;
        private readonly MemoryStream memoryStream;

        public int Length => (int)memoryStream.Length;

        public ByteArray(long initialCapacity)
        {
            var capacity = EnforceAllowedValues(initialCapacity);
            memoryStream = new MemoryStream(capacity);
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

        public void Prepend(byte[] prefix)
        {
            var oldLength = memoryStream.Length;

            var newLength = oldLength + prefix.Length;
            memoryStream.SetLength(newLength);

            var buffer = memoryStream.GetBuffer();
            buffer.ShiftBytesRight((int)oldLength, (int)newLength);

            Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
        }

        public void Append(byte[] buffer)
        {
            if (buffer.Length == 0)
                return;

            memoryStream.Write(buffer, 0, buffer.Length);
        }

        public void Reset()
        {
            memoryStream.SetLength(Zero);
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