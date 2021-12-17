using Finalspace.Onigiri.Models;
using System.Collections;

namespace Finalspace.Onigiri.Helper
{
    public class TitleSorter : IComparer
    {
        public int Compare(object ox, object oy)
        {
            if (ox == null || oy == null)
                return 0;
            if (!(ox is Title) || !(oy is Title))
                return 0;
            Title x = ox as Title;
            Title y = oy as Title;
            int result = string.Compare(x.Name, y.Name);
            return (result);
        }
    }
}
