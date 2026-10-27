using System;
using System.IO;
using System.Reflection;

using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rapr.Tests
{
    [TestClass]
    public class CsvExporterTests
    {
        private static readonly Func<string[], string, string[]> Sanitize =
            (Func<string[], string, string[]>)typeof(CsvExporter)
                .GetMethod("Sanitize", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<string[], string, string[]>));

        [TestMethod]
        [DataRow(",", "Acme \"Audio\", Inc.")]
        [DataRow(",", "line1\nline2")]
        [DataRow(",", "line1\rline2")]
        [DataRow(",", "line1\r\nline2")]
        [DataRow(";", "provider;name")]
        public void ExportedFieldsRoundTripAsOneRecord(string delimiter, string value)
        {
            string record = string.Join(delimiter, Sanitize(new[] { value, "second field", null }, delimiter));

            using (var reader = new TextFieldParser(new StringReader(record + Environment.NewLine)))
            {
                reader.TextFieldType = FieldType.Delimited;
                reader.SetDelimiters(delimiter);
                reader.HasFieldsEnclosedInQuotes = true;
                reader.TrimWhiteSpace = false;

                var fields = reader.ReadFields();

                Assert.AreEqual(3, fields.Length);
                Assert.AreEqual(NormalizeNewlines(value), NormalizeNewlines(fields[0]));
                Assert.AreEqual("second field", fields[1]);
                Assert.AreEqual(string.Empty, fields[2]);
                Assert.IsTrue(reader.EndOfData);
            }
        }

        [TestMethod]
        [DataRow("=cmd|' /C calc'!A0", "'=cmd|' /C calc'!A0")]
        [DataRow("+Provider", "'+Provider")]
        [DataRow("@SUM(1,1)", "'@SUM(1,1)")]
        [DataRow("-Provider", "'-Provider")]
        [DataRow("-1", "-1")]
        [DataRow("Contoso", "Contoso")]
        public void ValuesThatLookLikeFormulasAreEscaped(string value, string expected)
        {
            Assert.AreEqual(expected, Sanitize(new[] { value }, ";")[0]);
        }

        private static string NormalizeNewlines(string value)
        {
            return value.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}
