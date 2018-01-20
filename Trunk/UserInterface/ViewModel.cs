using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Data.DatabaseTableAdapters.AssetTypesTableAdapter _assetTypesTableAdapter = new Data.DatabaseTableAdapters.AssetTypesTableAdapter();
        public Data.DatabaseTableAdapters.AssetTypesTableAdapter AssetTypeAdapter { get { return _assetTypesTableAdapter; } }

        private Data.DatabaseTableAdapters.AssetsTableAdapter _assetAdapter = new Data.DatabaseTableAdapters.AssetsTableAdapter();
        public Data.DatabaseTableAdapters.AssetsTableAdapter AssetAdapter { get { return _assetAdapter; } }

        private Data.DatabaseTableAdapters.ManufacturersTableAdapter _manufacturerAdapter = new Data.DatabaseTableAdapters.ManufacturersTableAdapter();
        public Data.DatabaseTableAdapters.ManufacturersTableAdapter ManufacturerAdapter { get { return _manufacturerAdapter; } }

        private Data.DatabaseTableAdapters.StatesTableAdapter _statesAdapter = new Data.DatabaseTableAdapters.StatesTableAdapter();
        public Data.DatabaseTableAdapters.StatesTableAdapter StatesAdapter { get { return _statesAdapter; } }

        private Data.Database.AssetsDataTable _assetDataTable = new Data.Database.AssetsDataTable();
        public Data.Database.AssetsDataTable AssetDataTable
        {
            get { return _assetDataTable; }
            set { _assetDataTable = value; NotifyPropertyChanged("AssetDataTable"); }
        }

        private Data.Database.AssetTypesDataTable _assetTypeDataTable = new Data.Database.AssetTypesDataTable();
        public Data.Database.AssetTypesDataTable AssetTypeDataTable
        {
            get { return _assetTypeDataTable; }
            set { _assetTypeDataTable = value; NotifyPropertyChanged("AssetTypeDataTable"); }
        }

        private Data.Database.ManufacturersDataTable _manufacturerDataTable = new Data.Database.ManufacturersDataTable();
        public Data.Database.ManufacturersDataTable ManufacturerDataTable
        {
            get { return _manufacturerDataTable; }
            set { _manufacturerDataTable = value; NotifyPropertyChanged("ManufacturerDataTable"); }
        }

        private Data.Database.StatesDataTable _statesDataTable = new Data.Database.StatesDataTable();
        public Data.Database.StatesDataTable StatesDataTable
        {
            get { return _statesDataTable; }
            set { _statesDataTable = value; NotifyPropertyChanged("StatesDataTable"); }
        }

        private Data.Database.AssetsRow _assetRow = null;
        public Data.Database.AssetsRow AssetRow
        {
            get { return _assetRow; }
            set { _assetRow = value; NotifyPropertyChanged("AssetRow"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public void SetAssetDataSource(string asset)
        {
            AssetDataTable = AssetAdapter.GetDataByCode(asset);
        }

        public void Update()
        {
            AssetAdapter.Update(AssetDataTable);
        }
    }

    public class FormattedDecimalViewModel : INotifyPropertyChanged
    {
        private readonly string _format;
        private object _bindingObject;
        private string _bindingColumn;

        public FormattedDecimalViewModel(string format, object bindingObject, string bindingColumn)
        {
            _format = format;
            _bindingObject = bindingObject;
            _bindingColumn = bindingColumn;
        }

        private string _formattedString;
        public string FormattedString
        {
            get
            {
                if (_formattedString == null)
                {
                    if (_bindingObject.GetType() == typeof(Data.Database.AssetsRow))
                        return ((Data.Database.AssetsRow)_bindingObject)[_bindingColumn].ToString();
                }

                return _formattedString;
            }
            set
            {
                _formattedString = value;
                NotifyPropertyChanged("Value");
                _formattedString = Value.ToString(_format);
                NotifyPropertyChanged("FormattedString");

                if (_bindingObject.GetType() == typeof(Data.Database.AssetsRow))
                    ((Data.Database.AssetsRow)_bindingObject)[_bindingColumn] = Value;
            }
        }

        public decimal Value
        {
            get
            {
                if (_formattedString == null)
                    return 0m;
                return decimal.Parse(_formattedString.Replace("$", ""));
            }
            set
            {
                FormattedString = value.ToString(_format);
            }
        }

        public void ApplyFormat()
        {
            FormattedString = Value.ToString(_format);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
