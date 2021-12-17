using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services
{
    public interface IProcessStarterService
    {
        void Start(string executable, params string[] args);
    }
}
