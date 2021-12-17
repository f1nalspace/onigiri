using Finalspace.Onigiri.Models;
using System.Collections;

namespace Finalspace.Onigiri.Helper
{
    public class IssuesSorter : IComparer
    {
        public int Compare(object ox, object oy)
        {
            if (ox == null || oy == null)
                return 0;
            if (!(ox is Issue) || !(oy is Issue))
                return 0;
            Issue x = ox as Issue;
            Issue y = oy as Issue;
            int result = string.Compare(x.Path, y.Path);
            return (result);
        }
    }
}
