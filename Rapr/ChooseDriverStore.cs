using System;
using System.ComponentModel;
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
            }
        }

        public bool OKButtonEnable
        {
            get
            {
                return this.StoreType == DriverStoreType.Online
                    || !string.IsNullOrEmpty(this.OfflineStoreLocation);
            }
        }

        public ChooseDriverStore()
        {
            this.InitializeComponent();
            AddRadioCheckedBinding(this.radioButtonDriverStoreOnline, this, nameof(this.StoreType), DriverStoreType.Online);
            AddRadioCheckedBinding(this.radioButtonDriverStoreOffline, this, nameof(this.StoreType), DriverStoreType.Offline);

            this.textBoxOfflineStoreLocation.DataBindings.Add(
                nameof(this.textBoxOfflineStoreLocation.Enabled),
                this.radioButtonDriverStoreOffline,
                nameof(this.radioButtonDriverStoreOffline.Checked));

            this.textBoxOfflineStoreLocation.DataBindings.Add(
                nameof(this.textBoxOfflineStoreLocation.Text),
                this,
                nameof(this.OfflineStoreLocation));

            this.buttonBrowseLocation.DataBindings.Add(
                nameof(this.buttonBrowseLocation.Enabled),
                this.radioButtonDriverStoreOffline,
                nameof(this.radioButtonDriverStoreOffline.Checked));

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
                IsFolderPicker = true
            })
            {
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    this.OfflineStoreLocation = dialog.FileName;
                }
            }
        }
    }
}
