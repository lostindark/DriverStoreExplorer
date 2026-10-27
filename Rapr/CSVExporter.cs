using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr
{
    public class CsvExporter : IExport
    {
        private const string CsvQuote = "\"";

        // Spreadsheet apps treat a cell starting with one of these as a formula. Provider and
        // device names come from third-party INF files, so they shouldn't be able to become one.
        private static readonly char[] FormulaPrefixes = { '=', '+', '-', '@', '\t', '\r' };

        public string Export(List<DriverStoreEntry> driverStoreEntries)
        {
            if (driverStoreEntries == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntries));
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                DefaultExt = "csv",
                Filter = Language.Dialog_Export_Filters,
                SupportMultiDottedExtensions = true
            })
            {
                DialogResult dr = saveFileDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    var csvDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                    string csvFileName = saveFileDialog.FileName;

                    using (FileStream fileStream = new FileStream(csvFileName, FileMode.Create))
                    // Excel only reads the file as UTF-8 when it starts with a BOM; without it,
                    // non-ASCII provider and device names are garbled.
                    using (StreamWriter fileWriter = new StreamWriter(fileStream, new UTF8Encoding(true)))
                    {
                        // Write the header once
                        string headerLine = string.Join(csvDelimiter, Sanitize(DriverStoreEntry.GetFieldNames(), csvDelimiter));
                        fileWriter.WriteLine(headerLine);

                        // Write the values
                        foreach (DriverStoreEntry entry in driverStoreEntries)
                        {
                            string valueLine = string.Join(csvDelimiter, Sanitize(entry.GetFieldValues(), csvDelimiter));
                            fileWriter.WriteLine(valueLine);
                        }
                    }

                    return csvFileName;
                }
            }

            return null;
        }

        private static string[] Sanitize(string[] values, string csvDelimiter)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    values[i] = string.Empty;
                    continue;
                }

                if (values[i].Length > 0
                    && Array.IndexOf(FormulaPrefixes, values[i][0]) >= 0
                    && !double.TryParse(values[i], NumberStyles.Float, CultureInfo.CurrentCulture, out _))
                {
                    values[i] = "'" + values[i];
                }

                if (values[i].Contains(csvDelimiter)
                    || values[i].Contains(CsvQuote)
                    || values[i].Contains("\r")
                    || values[i].Contains("\n"))
                {
                    values[i] = CsvQuote + values[i].Replace(CsvQuote, CsvQuote + CsvQuote) + CsvQuote;
                }
            }

            return values;
        }
    }
}
