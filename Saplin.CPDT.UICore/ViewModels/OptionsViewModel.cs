using System;
using System.Globalization;
using Xamarin.Forms;

namespace Saplin.CPDT.UICore.ViewModels
{
    public class OptionsViewModel : PopupViewModel
    {
        private const string True = "true";
        private const string False = "false";

        public string MemCache
        {
            get
            {
                if (!Application.Current.Properties.ContainsKey(nameof(MemCache))) Application.Current.Properties[nameof(MemCache)] = False;
                return Application.Current.Properties[nameof(MemCache)] as string;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = False;

                if (value != False && value != True) throw new InvalidOperationException("Cant set MemCache to: "+value);
                Application.Current.Properties[nameof(MemCache)] = value;
                Application.Current.SavePropertiesAsync();
                RaisePropertyChanged();
            }
        }

        public bool MemCacheBool
        {
            get { return MemCache == True; }
        }

        public string Csv
        {
            get
            {
                if (!App.Current.Properties.ContainsKey(nameof(Csv))) App.Current.Properties[nameof(Csv)] = False;
                return App.Current.Properties[nameof(Csv)] as string;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = False;

                if (value != False && value != True) throw new InvalidOperationException("Cant set Csv to: " + value);
                App.Current.Properties[nameof(Csv)] = value;
                App.Current.SavePropertiesAsync();
                RaisePropertyChanged();
            }
        }

        public bool CsvBool
        {
            get { return Csv == True; }
        }

        public string WriteBuffering
        {
            get
            {
                if (!App.Current.Properties.ContainsKey(nameof(WriteBuffering))) App.Current.Properties[nameof(WriteBuffering)] = False;
                return App.Current.Properties[nameof(WriteBuffering)] as string;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = False;

                if (value != False && value != True) throw new InvalidOperationException("Cant set WriteBuffering to: " + value);
                App.Current.Properties[nameof(WriteBuffering)] = value;
                App.Current.SavePropertiesAsync();
                RaisePropertyChanged();
            }
        }

        public bool WriteBufferingBool
        {
            get { return WriteBuffering == True; }
        }

        public string WhiteTheme
        {
            get
            {
                if (!App.Current.Properties.ContainsKey(nameof(WhiteTheme))) App.Current.Properties[nameof(WhiteTheme)] = False;
                return App.Current.Properties[nameof(WhiteTheme)] as string;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = False;

                if (value != False && value != True) throw new InvalidOperationException("Cant set WhiteTheme to: " + value);
                App.Current.Properties[nameof(WhiteTheme)] = value;
                App.Current.SavePropertiesAsync();
                RaisePropertyChanged();
            }
        }

        public bool WhiteThemeBool
        {
            get { return WhiteTheme == True; }
        }

        private readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = "."};

        public string FileSizeGb
        {
            get
            {
                if (!App.Current.Properties.ContainsKey(nameof(FileSizeGb))) App.Current.Properties[nameof(FileSizeGb)] = 1;
                return App.Current.Properties[nameof(FileSizeGb)].ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = "1";

                if (App.Current.Properties[nameof(FileSizeGb)] as string != value)
                {
                    App.Current.Properties[nameof(FileSizeGb)] = value;

                    App.Current.SavePropertiesAsync();
                    RaisePropertyChanged();
                }
            }
        }

        public string IID
        {
            get
            {
                if (!Application.Current.Properties.ContainsKey("IID")) Application.Current.Properties["IID"] = Guid.NewGuid().ToString("N");
                return Application.Current.Properties["IID"] as string;
            }
        }

        public long FileSizeBytes
        {
            get
            {
                float i;
                float.TryParse(FileSizeGb, NumberStyles.Any, nfi, out i);

                return (long)(i * 1024 * 1024 * 1024);
            }
        }
    }
}
