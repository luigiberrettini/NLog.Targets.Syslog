// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Text;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class EncodingSet
    {
        public ASCIIEncoding Ascii { get; }
        public UTF8Encoding Utf8 { get; }

        public EncodingSet(bool enableBom)
        {
            Ascii = new ASCIIEncoding();
            Utf8 = new UTF8Encoding(enableBom);
        }
    }
}