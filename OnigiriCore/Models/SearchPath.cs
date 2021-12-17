using Finalspace.Onigiri.MVVM;
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
            get { return GetValue(() => Path); }
            set { SetValue(() => Path, value); }
        }

        [XmlAttribute("drive")]
        public string DriveName
        {
            get { return GetValue(() => DriveName); }
            set { SetValue(() => DriveName, value); }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DriveName))
                return $"Path ({DriveName})";
            return Path;
        }
    }
}
