using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Common.Helpers
{
    public static class XmlCompressor
    {
        public static string Compress(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return xml;

            var bytes = Encoding.UTF8.GetBytes(xml);
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decompress(string compressedXml)
        {
            if (string.IsNullOrEmpty(compressedXml)) return compressedXml;

            var bytes = Convert.FromBase64String(compressedXml);
            using var memoryStream = new MemoryStream(bytes);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gzipStream.CopyTo(resultStream);
            return Encoding.UTF8.GetString(resultStream.ToArray());
        }
    }
}
