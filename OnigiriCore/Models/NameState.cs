using DevExpress.Mvvm;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class NameState: BindableBase
    {
        [XmlAttribute("name")]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute("value")]
        public bool Value
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
    }
}
