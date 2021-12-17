using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Utils
{
    static class AnimeUtils
    {
        public static string GetCleanAnimeName(string path)
        {
            string s = path;
            if (!string.IsNullOrEmpty(s))
            {
                s = s.Replace(".", "");
                s = s.Replace(":", "");
                s = s.Replace("?", "");
                s = s.Replace("&", "");
                s = s.Replace("|", "");
                s = s.Replace("<", "");
                s = s.Replace(">", "");
                s = s.Replace("/", "");
                s = s.Replace("\\", "");
                s = s.Replace(";", "");
            }
            return s;
        }
    }
}
