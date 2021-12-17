using DevExpress.Mvvm;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class SearchPath : BindableBase
    {
        [XmlText]
        public string Path
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute("drive")]
        public string DriveName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DriveName))
                return $"Path ({DriveName})";
            return Path;
        }
    }
}
