using System.Collections.Generic;

namespace Finalspace.Onigiri.Types
{
    public enum LanguageKind
    {
        Unknown = 0,
        Japanese,
        English,
        German,
    }

    public static class LanguageNames
    {
        public const string EnglishShort = "en";
        public const string EnglishLong = "english";
        public const string JapaneseShort = "jp";
        public const string JapaneseLang = "japanese";
        public const string GermanShort = "de";
        public const string GermanLong = "german";

        private static readonly Dictionary<string, LanguageKind> ValueToKindMap = new Dictionary<string, LanguageKind>()
        {
            { EnglishShort, LanguageKind.English },
            { EnglishLong, LanguageKind.English },
            { JapaneseShort, LanguageKind.Japanese },
            { JapaneseLang, LanguageKind.Japanese },
            { GermanShort, LanguageKind.German },
            { GermanLong, LanguageKind.German },
        };

        public static string ValueFromKind(LanguageKind kind)
        {
            return kind switch
            {
                LanguageKind.Japanese => JapaneseShort,
                LanguageKind.English => EnglishShort,
                LanguageKind.German => GermanShort,
                _ => null,
            };
        }

        public static LanguageKind ValueToKind(string value)
        {
            var test = value?.ToLower() ?? string.Empty;
            if (ValueToKindMap.TryGetValue(test, out LanguageKind kind))
                return kind;
            return LanguageKind.Unknown;
        }
    }

    public readonly struct LanguageType
    {
        public LanguageKind Kind { get; }
        public string Value { get; }

        public LanguageType(LanguageKind kind)
        {
            Kind = kind;
            Value = LanguageNames.ValueFromKind(kind);
        }

        public LanguageType(LanguageKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public LanguageType(string value)
        {
            Kind = LanguageNames.ValueToKind(value);
            Value = value;
        }
    }
}
