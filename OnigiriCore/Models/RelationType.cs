namespace Finalspace.Onigiri.Models
{
    public enum RelationType
    {
        None,
        Sequel,
        Prequel,
        Alternative,
        Other,
    }

    public static class RelationTypeConverter
    {
        public static RelationType FromString(string source)
        {
            switch (source.ToLower())
            {
                case "sequel":
                    return RelationType.Sequel;
                case "prequel":
                    return RelationType.Prequel;
                default:
                    return RelationType.None;
            }
        }
    }
}
