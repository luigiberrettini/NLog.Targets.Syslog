using System;
using System.IO;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog
{
    public class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private readonly MemoryStream memoryStream;

        public int Length => (int)memoryStream.Length;

        public ByteArray(int initialCapacity)
        {
            var capacity = initialCapacity == 0 ? DefaultBufferCapacity : initialCapacity;
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

        public void Resize(int newLength)
        {
            memoryStream.SetLength(newLength);
        }

        public void Dispose()
        {
            memoryStream.SetLength(Zero);
            memoryStream.Capacity = Zero;
            memoryStream.Dispose();
        }
    }
}