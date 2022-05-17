using DevExpress.Mvvm;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot]
    public class User : BindableBase
    {
        [XmlAttribute]
        public string UserName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public string DisplayName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public string ActiveImage
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public string DisabledImage
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public User(string name, string activeImage, string disabledImage)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(activeImage))
                throw new ArgumentException(nameof(activeImage));
            if (string.IsNullOrWhiteSpace(disabledImage))
                throw new ArgumentException(nameof(disabledImage));
            UserName = DisplayName = name;
            ActiveImage = activeImage;
            DisabledImage = disabledImage;
        }

        public override string ToString() => DisplayName;
    }
}
