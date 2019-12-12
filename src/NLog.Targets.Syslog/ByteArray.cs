// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Text;

namespace NLog.Targets.Syslog
{
    internal class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private const int MaxBufferCapacity = int.MaxValue;
        private const int MaxEncodingBufferCapacity = 1024;
        private readonly MemoryStream memoryStream;
        private readonly char[] encodingBuffer;

        public int Length => (int)memoryStream.Length;

        public ByteArray(long initialCapacity)
        {
            var capacity = EnforceAllowedValues(initialCapacity);
            memoryStream = new MemoryStream(capacity);
            encodingBuffer = new char[MaxEncodingBufferCapacity];
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

        public void Append(string buffer, Encoding encoding)
        {
            if (string.IsNullOrEmpty(buffer))
                return;

            var byteCount = encoding.GetByteCount(buffer);
            memoryStream.SetLength(memoryStream.Length + byteCount);
            for (int i = 0; i < buffer.Length; i += encodingBuffer.Length)
            {
                int remainingCount = Math.Min(buffer.Length - i, encodingBuffer.Length);
                buffer.CopyTo(i, encodingBuffer, 0, remainingCount);
                memoryStream.Position += encoding.GetBytes(encodingBuffer, 0, remainingCount, memoryStream.GetBuffer(), (int)memoryStream.Position);
            }
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