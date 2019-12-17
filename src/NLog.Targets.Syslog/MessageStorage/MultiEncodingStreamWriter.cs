// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLog.Targets.Syslog.MessageStorage
{
    internal class MultiEncodingStreamWriter : IDisposable
    {
        private readonly Dictionary<Encoding, StreamWriter> streamWriterForEncodingType;

        public MultiEncodingStreamWriter(Stream stream)
        {
            var ascii = new StreamWriter(stream, new ASCIIEncoding()) { AutoFlush = true };
            var utf8 = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            streamWriterForEncodingType = new Dictionary<Encoding, StreamWriter>
            {
                { Encoding.ASCII, ascii },
                { Encoding.UTF8, utf8 }
            };
        }

        public void Write(Encoding encoding, string s)
        {
            streamWriterForEncodingType[encoding].Write(s);
        }

        public void Dispose()
        {
            foreach (var streamWriter in streamWriterForEncodingType.Values)
                streamWriter.Dispose();
        }
    }
}