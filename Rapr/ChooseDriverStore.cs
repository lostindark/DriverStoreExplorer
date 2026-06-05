using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace Rapr
{
    public partial class ChooseDriverStore : Form, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string fieldName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));

        private DriverStoreType storeType;

        public DriverStoreType StoreType
        {
            get
            {
                return this.storeType;
            }
            set
            {
                this.storeType = value;
                this.NotifyPropertyChanged(nameof(this.StoreType));
                // Crucial: Let the OK button know that changing the store type changes validation state
                this.NotifyPropertyChanged(nameof(this.OKButtonEnable));
            }
        }

        private string offlineStoreLocation;

        public string OfflineStoreLocation
        {
            get
            {
                return this.offlineStoreLocation;
            }
            set
            {
                this.offlineStoreLocation = value;
                this.NotifyPropertyChanged(nameof(this.OfflineStoreLocation));
                this.NotifyPropertyChanged(nameof(this.OKButtonEnable));
            }
        }

        public bool OKButtonEnable
        {
            get
            {
                return this.StoreType == DriverStoreType.Online
                    || (!string.IsNullOrEmpty(this.OfflineStoreLocation) && IsValidStoreLocation(this.OfflineStoreLocation, this.StoreType));
            }
        }

        public ChooseDriverStore()
        {
            this.InitializeComponent();
            
            // 1. Bind our radio buttons to the backing StoreType property
            AddRadioCheckedBinding(this.radioButtonDriverStoreOnline, this, nameof(this.StoreType), DriverStoreType.Online);
            AddRadioCheckedBinding(this.radioButtonDriverStoreOffline, this, nameof(this.StoreType), DriverStoreType.Offline);
            AddRadioCheckedBinding(this.radioButtonDriverStoreCustomFolder, this, nameof(this.StoreType), DriverStoreType.CustomFolder);

            // 2. Bind TextBox Enabled property to the backing StoreType enum using a Format converter
            Binding textBoxEnabledBinding = new Binding(
                nameof(this.textBoxOfflineStoreLocation.Enabled),
                this,
                nameof(this.StoreType),
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            textBoxEnabledBinding.Format += (s, le) => {
                if (le.Value is DriverStoreType type)
                {
                    le.Value = (type == DriverStoreType.Offline || type == DriverStoreType.CustomFolder);
                }
            };
            this.textBoxOfflineStoreLocation.DataBindings.Add(textBoxEnabledBinding);

            // 3. Bind Browse Button Enabled property using the same Format converter logic
            Binding buttonEnabledBinding = new Binding(
                nameof(this.buttonBrowseLocation.Enabled),
                this,
                nameof(this.StoreType),
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            buttonEnabledBinding.Format += (s, le) => {
                if (le.Value is DriverStoreType type)
                {
                    le.Value = (type == DriverStoreType.Offline || type == DriverStoreType.CustomFolder);
                }
            };
            this.buttonBrowseLocation.DataBindings.Add(buttonEnabledBinding);

            // 4. Bind Text and OK button state directly
            this.textBoxOfflineStoreLocation.DataBindings.Add(
                nameof(this.textBoxOfflineStoreLocation.Text),
                this,
                nameof(this.OfflineStoreLocation));

            this.buttonOK.DataBindings.Add(
                nameof(this.buttonOK.Enabled),
                this,
                nameof(this.OKButtonEnable));
        }

        private static void AddRadioCheckedBinding<T>(RadioButton radio, object dataSource, string dataMember, T trueValue)
        {
            var binding = new Binding(nameof(RadioButton.Checked), dataSource, dataMember, true, DataSourceUpdateMode.OnPropertyChanged);

            binding.Parse += (s, arg) =>
            {
                if ((bool)arg.Value)
                {
                    arg.Value = trueValue;
                }
            };

            binding.Format += (s, arg) => arg.Value = ((T)arg.Value).Equals(trueValue);

            radio.DataBindings.Add(binding);
        }

        private void ButtonBrowseLocation_Click(object sender, EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = this.StoreType == DriverStoreType.CustomFolder 
                    ? "Select Custom Driver Folder" 
                    : Lang.Language.Dialog_Select_Offline_Store_Title,
            })
            {
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    string selectedPath = dialog.FileName;
                    
                    // Validate the selected directory based on store type
                    if (!IsValidStoreLocation(selectedPath, this.StoreType))
                    {
                        string message = this.StoreType == DriverStoreType.CustomFolder
                            ? "Please select a valid folder for storing drivers."
                            : Lang.Language.Message_Invalid_Offline_Store_Location;

                        MessageBox.Show(
                            this,
                            message,
                            Lang.Language.Message_Invalid_Offline_Store_Title,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    
                    this.OfflineStoreLocation = selectedPath;
                }
            }
        }

        /// <summary>
        /// Validates that the selected directory is valid for the specified store type.
        /// For Offline stores, checks for Windows folder.
        /// For Custom Folder stores, just checks if the directory exists.
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <param name="storeType">The type of store being used</param>
        /// <returns>True if the path is valid for the store type, false otherwise</returns>
        private static bool IsValidStoreLocation(string path, DriverStoreType storeType)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }

            // For custom folder stores, just verify the directory exists
            if (storeType == DriverStoreType.CustomFolder)
            {
                return true;
            }

            // For offline stores, check if the selected directory contains a "Windows" folder
            string windowsPath = Path.Combine(path, "Windows");
            return Directory.Exists(windowsPath);
        }
    }
}