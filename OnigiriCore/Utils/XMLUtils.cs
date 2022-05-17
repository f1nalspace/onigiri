using System;
using System.Globalization;
using System.Xml;

namespace Finalspace.Onigiri.Utils
{
    static class XMLUtils
    {
        private static T GetValue<T>(string value, T def, string format = null)
        {
            if (typeof(T) == typeof(DateTime?) && !string.IsNullOrEmpty(format))
            {
                DateTime dt;
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    Type t = typeof(T);
                    t = Nullable.GetUnderlyingType(t) ?? t;
                    return (T)Convert.ChangeType(dt, t);
                }
                else return def;
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                    return (T)Convert.ChangeType(result, typeof(T));
                else
                    return def;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static T GetAttribute<T>(this XmlNode node, string name, T def, string format = null)
        {
            XmlNode attr = node.Attributes.GetNamedItem(name);
            if (attr != null)
                return GetValue(attr.Value, def, format);
            return def;
        }
        public static T GetAttribute<T>(this XmlNode rootNode, string xpath, string name, T def, string format = null)
        {
            XmlNode node = rootNode.SelectSingleNode(xpath);
            if (node != null)
                return GetAttribute(node, name, def);
            return def;
        }

        public static T GetValue<T>(this XmlNode node, string xpath, T def, string format = null)
        {
            XmlNode found = node.SelectSingleNode(xpath);
            if (found != null)
                return GetValue(found.InnerText, def, format);
            return def;
        }
    }
}
