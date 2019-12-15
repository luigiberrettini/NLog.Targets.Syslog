// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLog.Targets.Syslog
{
    internal class MultiEncodingStreamWriter : IDisposable
    {
        private readonly Dictionary<Type, StreamWriter> streamWriterForEncodingType;

        public MultiEncodingStreamWriter(Stream stream)
        {
            var ascii = new StreamWriter(stream, new ASCIIEncoding()) { AutoFlush = true };
            var utf8 = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            streamWriterForEncodingType = new Dictionary<Type, StreamWriter>
            {
                { typeof(ASCIIEncoding), ascii },
                { Encoding.ASCII.GetType(), ascii },
                { typeof(UTF8Encoding), utf8 },
                { Encoding.UTF8.GetType(), utf8 }
            };
        }

        public void Write(Encoding encoding, string s)
        {
            streamWriterForEncodingType[encoding.GetType()].Write(s);
        }

        public void Dispose()
        {
            foreach (var streamWriter in streamWriterForEncodingType.Values)
                streamWriter.Dispose();
        }
    }
}