using Finalspace.Onigiri.MVVM;
using log4net;
using System;
using System.Reflection;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class SearchTypeLanguage: BindableBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("lang")]
        public string Lang { get; set; }
    }
}
