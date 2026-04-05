using Finalspace.Onigiri.MVVM;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class UserState: BindableBase
    {
        [XmlAttribute("name")]
        public string UserName
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
