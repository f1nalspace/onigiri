using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.ViewModels
{
    public class WatchStateItemViewModel: NameItemViewModel
    {
        public string FilterProperty
        {
            get { return GetValue(() => FilterProperty); }
            set { SetValue(() => FilterProperty, value); }
        }
    }
}
