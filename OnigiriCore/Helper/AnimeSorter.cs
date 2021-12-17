using Finalspace.Onigiri.Models;
using System.Collections;

namespace Finalspace.Onigiri.Helper
{
    public enum AnimeSortKey
    {
        None = 0,
        Title,
        Rating,
        StartDate,
        EndDate,
    }

    public class AnimeSorter : IComparer
    {
        public AnimeSortKey FirstSortKey { get; set; }
        public AnimeSortKey SecondSortKey { get; set; }
        public bool FirstSortIsDesc { get; set; }
        public bool SecondSortIsDesc { get; set; }

        public int Compare(object ox, object oy)
        {
            if (ox == null || oy == null)
                return 0;
            if (!(ox is Anime) || !(oy is Anime))
                return 0;

            Anime x = ox as Anime;
            Anime y = oy as Anime;

            int result = 0;
            if (FirstSortKey != AnimeSortKey.None)
            {
                int order = FirstSortIsDesc ? -1 : 1;
                switch (FirstSortKey)
                {
                    case AnimeSortKey.Title:
                        result = x.MainTitle.CompareTo(y.MainTitle);
                        break;
                    case AnimeSortKey.Rating:
                        result = x.PermanentRating.CompareTo(y.PermanentRating);
                        break;
                    case AnimeSortKey.StartDate:
                        if (x.StartDate.HasValue && y.StartDate.HasValue)
                            result = x.StartDate.Value.CompareTo(y.StartDate.Value);
                        break;
                    case AnimeSortKey.EndDate:
                        if (x.EndDate.HasValue && y.EndDate.HasValue)
                            result = x.EndDate.Value.CompareTo(y.EndDate.Value);
                        break;
                }
                result *= order;
            }
            if (SecondSortKey != AnimeSortKey.None && result == 0)
            {
                int order = SecondSortIsDesc ? -1 : 1;
                switch (SecondSortKey)
                {
                    case AnimeSortKey.Title:
                        result = x.MainTitle.CompareTo(y.MainTitle);
                        break;
                    case AnimeSortKey.Rating:
                        result = x.PermanentRating.CompareTo(y.PermanentRating);
                        break;
                    case AnimeSortKey.StartDate:
                        if (x.StartDate.HasValue && y.StartDate.HasValue)
                            result = x.StartDate.Value.CompareTo(y.StartDate.Value);
                        break;
                    case AnimeSortKey.EndDate:
                        if (x.EndDate.HasValue && y.EndDate.HasValue)
                            result = x.EndDate.Value.CompareTo(y.EndDate.Value);
                        break;
                }
                result *= order;
            }
            return result;
        }
    }
}
