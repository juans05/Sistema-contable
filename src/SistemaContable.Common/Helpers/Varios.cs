using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SistemaContable.Common.Helpers
{
    public static class Varios
    {
        public static string CalcularHash(string xmlContent)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
            return Convert.ToBase64String(bytes);
        }

        public static string NormalizarXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return xml;

            try
            {
                // 1. Asegurar que esté en UTF-8
                var bytes = Encoding.UTF8.GetBytes(xml);
                xml = Encoding.UTF8.GetString(bytes);

                // 2. Corregir declaración XML si es necesario
                if (!xml.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                {
                    xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xml;
                }

                // 3. Remover caracteres de control inválidos
                xml = System.Text.RegularExpressions.Regex.Replace(
                    xml,
                    @"[\x00-\x08\x0B\x0C\x0E-\x1F]",
                    string.Empty
                );

                // 4. Validar que sea XML válido
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo normalizar XML, usando original", ex.Message);
                return xml;
            }
        }

        public static string LimpiarXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return xml;

            // Remover caracteres de control problemáticos
            xml = Regex.Replace(xml, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

            // Normalizar espacios
            xml = Regex.Replace(xml, @"\s+", " ");

            return xml.Trim();
        }
    }
}
