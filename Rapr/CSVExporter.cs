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
                    using (StreamWriter fileWriter = new StreamWriter(fileStream, new UTF8Encoding(false)))
                    {
                        // Write the header once
                        string headerLine = string.Join(csvDelimiter, DriverStoreEntry.GetFieldNames());
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
                if (values[i].Contains(csvDelimiter)
                    || values[i].Contains(CsvQuote))
                {
                    values[i] = CsvQuote + values[i].Replace(CsvQuote, "\\\"") + CsvQuote;
                }
            }

            return values;
        }
    }
}
