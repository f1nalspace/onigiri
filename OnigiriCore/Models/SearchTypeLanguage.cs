using Finalspace.Onigiri.MVVM;
using Serilog;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class SearchTypeLanguage : BindableBase
    {
        private static readonly ILogger log = Log.ForContext<SearchTypeLanguage>();

        [XmlAttribute("type")]
        public string Type
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute("lang")]
        public string Lang
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}
