// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Text;

namespace NLog.Targets.Syslog.MessageStorage
{
    internal class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private const int MaxBufferCapacity = int.MaxValue;
        private readonly MemoryStream memoryStream;
        private readonly MultiEncodingStreamWriter streamWriter;

        public int Length => (int)memoryStream.Length;

        public ByteArray(long initialCapacity)
        {
            var capacity = EnforceAllowedValues(initialCapacity);
            memoryStream = new MemoryStream(capacity);
            streamWriter = new MultiEncodingStreamWriter(memoryStream);
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

        public void AppendAscii(string s)
        {
            Append(s, Encoding.ASCII);
        }

        public void AppendUtf8(string s)
        {
            Append(s, Encoding.UTF8);
        }

        public void AppendBytes(byte[] buffer)
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

        private void Append(string s, Encoding encoding)
        {
            if (s.Length == 0)
                return;

            streamWriter.Write(encoding, s);
        }

        public void Dispose()
        {
            memoryStream.SetLength(Zero);
            memoryStream.Capacity = Zero;
            streamWriter.Dispose();
        }
    }
}