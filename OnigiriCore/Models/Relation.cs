using Finalspace.Onigiri.MVVM;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class Relation : BindableBase
    {
        [XmlAttribute()]
        public ulong Aid
        {
            get { return GetValue(() => Aid); }
            set { SetValue(() => Aid, value); }
        }

        [XmlAttribute("type")]
        public string TypeStr
        {
            get { return GetValue(() => TypeStr); }
            set { SetValue(() => TypeStr, value, () => { Type = RelationTypeConverter.FromString(value); }); }
        }

        [XmlIgnore]
        public RelationType Type
        {
            get { return GetValue(() => Type); }
            private set { SetValue(() => Type, value); }
        }

        [XmlAttribute()]
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
