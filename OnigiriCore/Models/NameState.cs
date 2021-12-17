using Finalspace.Onigiri.MVVM;
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
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        [XmlAttribute("value")]
        public bool Value
        {
            get { return GetValue(() => Value); }
            set { SetValue(() => Value, value); }
        }
    }
}
