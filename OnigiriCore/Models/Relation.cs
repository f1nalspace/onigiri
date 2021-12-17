using DevExpress.Mvvm;
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
            get { return GetValue<ulong>(); }
            set { SetValue(value); }
        }

        [XmlAttribute("type")]
        public string TypeStr
        {
            get { return GetValue<string>(); }
            set { SetValue(value, () => { Type = RelationTypeConverter.FromString(value); }); }
        }

        [XmlIgnore]
        public RelationType Type
        {
            get { return GetValue<RelationType>(); }
            private set { SetValue(value); }
        }

        [XmlAttribute()]
        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
