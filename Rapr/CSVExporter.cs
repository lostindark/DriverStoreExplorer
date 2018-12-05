using System;
using System.Collections.Generic;
using Rapr.Utils;
using System.Windows.Forms;
using Rapr.Lang;

namespace Rapr
{
    public class CSVExporter : IExport
    {
        private const string CSV_DELIM = ",";

        public string Export(List<DriverStoreEntry> ldse)
        {
            if (ldse.Count == 0)
            {
                MessageBox.Show(Language.Message_No_Entries, Language.Export_Error);
                return null;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                DefaultExt = "csv",
                Filter = Language.Dialog_Export_Filters,
                SupportMultiDottedExtensions = true
            };

            DialogResult dr = saveFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string csvFileName = saveFileDialog.FileName;   // Path                

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvFileName))
                {
                    // Write the header once
                    string headerLine = String.Join(CSV_DELIM, ldse[0].GetFieldNames());
                    file.WriteLine(headerLine);

                    // Write the values
                    foreach (DriverStoreEntry dse in ldse)
                    {
                        string[] values = dse.GetFieldValues();
                        this.Sanitize(ref values);

                        string valueLine = String.Join(CSV_DELIM, values);
                        file.WriteLine(valueLine);
                    }
                }

                return csvFileName;
            }

            return null;
        }

        private void Sanitize(ref string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Contains(CSV_DELIM))
                {
                    values[i] = "\"" + values[i] + "\"";//.Replace(CSV_DELIM, CSV_DELIM_SUBST);
                }
            }
        }
    }
}
